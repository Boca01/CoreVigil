# Mejoras y decisiones de implementación

Este documento explica por qué se realizaron ciertos cambios en CoreVigil, qué comportamiento buscan y cómo verificarlos. No sustituye a la documentación de arquitectura ni a las pruebas automatizadas.

## Muestreo de procesos fuera del hilo de interfaz

### Problema

La enumeración y lectura de procesos es síncrona y puede tardar más que una operación normal de interfaz, especialmente si hay muchos procesos o alguno tarda en responder. El ciclo del `MainViewModel` se inicia desde una ventana WPF y sus continuaciones vuelven al contexto de interfaz; por tanto, hacer allí ese trabajo podía provocar pausas visibles.

### Decisión

La llamada a `IProcessMonitor.GetTopProcessesAsync` se ejecuta en un trabajador mediante `Task.Run`. La adquisición sigue siendo secuencial: `ProcessMonitor` necesita comparar cada captura con la anterior para calcular consumo reciente, por lo que ejecutar capturas simultáneas alteraría sus muestras. Una vez terminada la operación, el ViewModel continúa en el contexto de WPF y actualiza las propiedades enlazadas desde el hilo de interfaz.

### Efecto esperado

La lectura periódica de procesos deja de ocupar el hilo que dibuja e interactúa con la ventana. El resto del ciclo conserva la misma frecuencia y el mismo cálculo de Top 8.

## Resistencia del diagnóstico y escritura CSV

### Problema

El escritor retiraba las muestras del búfer antes de completar la escritura. Un error de disco podía perderlas; además, una excepción no relacionada con cancelación podía finalizar la tarea de diagnóstico sin que el HUD explicara el motivo.

### Decisión

`RuntimeLogWriter` serializa los vaciados, permite añadir muestras concurrentemente y solo retira un lote del búfer después de que la escritura, el vaciado y el cierre del archivo hayan terminado correctamente. Si falla la operación, el lote sigue disponible para un nuevo intento. El ciclo de diagnóstico registra la excepción en `Debug`, espera cinco segundos y vuelve a intentarlo sin cerrar el HUD. El apagado también captura errores del vaciado final y conserva el escritor en memoria si no pudo persistir el lote.

### Efecto esperado y límite

Un fallo transitorio de acceso al archivo no detiene la toma de muestras y el mismo proceso puede reintentarlo. Los detalles quedan disponibles en la salida de depuración de Visual Studio. Un cierre forzado o permanente del proceso antes de recuperar el acceso a disco puede perder las muestras que solo existan en memoria; tampoco se puede garantizar atomicidad si el sistema de archivos falla después de escribir parcialmente un lote.

El búfer tiene un máximo de 10.000 muestras para que un fallo prolongado del disco no produzca crecimiento ilimitado de memoria. Al llegar al límite se descartan las muestras más antiguas, se incrementa un contador y se muestra un aviso en el estado del HUD Full y en el texto emergente del icono de bandeja. El aviso no genera globos repetidos. La pérdida se mantiene señalada después de recuperar la escritura, porque el CSV ya no contiene esas muestras.

## Errores visibles del diagnóstico

### Problema

Los errores solo se enviaban a `Debug`, una salida que normalmente no está disponible para quien ejecuta la aplicación instalada. Los errores repetidos tampoco debían convertirse en una sucesión de ventanas emergentes.

### Decisión

El estado de diagnóstico se muestra en el texto de estado del HUD Full y en el tooltip del icono de bandeja, también cuando CoreVigil está en Mini. Se informa del fallo, el reintento y la pérdida de muestras sin mostrar cuadros de diálogo; los detalles técnicos completos siguen en la salida `Debug`.

## Mutex de instancia única

### Problema

Una instancia secundaria obtiene un handle del mutex existente, pero no adquiere su propiedad. Llamar a `ReleaseMutex` desde esa instancia puede lanzar una excepción al cerrar.

### Decisión

`SingleInstanceGuard` intenta adquirir el mutex de forma no bloqueante y recuerda si lo obtuvo. Solo el propietario lo libera; un mutex abandonado se considera adquirido por el nuevo proceso, tal como indica Windows. El guard es testeable sin iniciar WPF.

### Verificación

La prueba `TryAcquire_SoloLaInstanciaPropietariaLiberaElMutex` comprueba que una segunda instancia no libera el mutex de la primera y que un nuevo proceso puede adquirirlo después de que el propietario lo libere.

## Cancelación del muestreo de procesos

### Problema

La enumeración de procesos se ejecutaba en segundo plano, pero el token solo se comprobaba antes de empezar. Al cerrar la ventana, una captura con muchos procesos podía seguir recorriendo toda la lista.

### Decisión

`ProcessMonitor` comprueba la cancelación entre procesos y dispone también los objetos que aún no se habían visitado si se interrumpe el recorrido. El ViewModel comprueba el token al volver del trabajador y no publica una lista después de solicitar el cierre.

La llamada nativa `Process.GetProcesses()` en sí no se puede interrumpir; la cancelación actúa tan pronto como devuelve el array o antes del siguiente elemento.

## Errores al aplicar Alt+Tab

Las llamadas `GetWindowLongPtr`, `SetWindowLongPtr` y `SetWindowPos` revisan sus códigos de error de Windows y registran el fallo en `Debug`, sin cerrar el HUD. El comportamiento normal de Full y Mini se conserva.

## Precisión de la frecuencia actual de CPU

### Problema

El contador PDH `% Processor Performance` necesita dos recopilaciones para calcular un porcentaje. La primera llamada devolvía 100 % por convención, lo que hacía que CoreVigil mostrase la frecuencia máxima sin haberla medido. Además, PDH puede indicar que el dato es válido con `PDH_CSTATUS_NEW_DATA` (1), además de `PDH_CSTATUS_VALID_DATA` (0); rechazar el primer estado podía descartar una lectura válida.

### Decisión

Mientras PDH aún no tiene un intervalo válido, CoreVigil utiliza el promedio de `CurrentMhz` que Windows devuelve mediante `CallNtPowerInformation`. Cuando PDH entrega una muestra válida, estima la frecuencia como `MaxMhz * % Processor Performance / 100`. Se aceptan ambos estados PDH válidos y se usa de nuevo la lectura nativa si el dato de rendimiento no es finito o no está disponible. No se limita el resultado a `MaxMhz`, porque el rendimiento turbo puede superar la frecuencia nominal informada.

### Precisión y límites

Esto evita presentar la frecuencia máxima como si fuera una muestra real al inicio y aprovecha las lecturas dinámicas posteriores. Sigue siendo información proporcionada por Windows y un cálculo agregado sobre procesadores lógicos, no una medición independiente de la señal física del reloj; por eso puede diferir algo entre hardware y del valor suavizado del Administrador de tareas.

Las pruebas deterministas cubren el fallback a `CurrentMhz`, el cálculo a partir de PDH, valores turbo superiores al máximo nominal y muestras no finitas. Para comparar con el equipo real, conviene observar ambos valores durante una carga estable y después de cambiar entre reposo y carga.

## Visibilidad en Alt+Tab por modo

### Requisito

Full debe comportarse como una ventana normal y aparecer en Alt+Tab. Mini debe seguir accesible desde la bandeja, pero no aparecer en Alt+Tab ni ocupar un botón de ventana normal.

### Decisión

Se conserva `ShowInTaskbar` para el comportamiento de la barra de tareas y, además, se establecen explícitamente los estilos extendidos de Windows: Full usa `WS_EX_APPWINDOW` y elimina `WS_EX_TOOLWINDOW`; Mini usa `WS_EX_TOOLWINDOW` y elimina `WS_EX_APPWINDOW`. Después se solicita a Windows recalcular los estilos. Se aplica al crear el identificador de ventana, al cambiar de modo y al restaurar la preferencia guardada.

### Verificación manual en Windows

1. Iniciar CoreVigil en modo Full y abrir Alt+Tab: CoreVigil debe aparecer.
2. Cambiar a Mini y abrir Alt+Tab: CoreVigil no debe aparecer; el icono y el menú de bandeja deben seguir disponibles.
3. Volver a Full: CoreVigil debe volver a aparecer en Alt+Tab.
4. Cerrar y abrir la aplicación después de guardar cada modo y confirmar que el modo restaurado conserva el comportamiento esperado.

El usuario ya confirmó manualmente que Full aparece y Mini queda excluido de Alt+Tab. Las comprobaciones de restauración y disponibilidad del icono de bandeja siguen siendo recomendables después de cambios en el ciclo de vida de la ventana.

## Verificación automatizada

La prueba `FlushAsync_SiFallaConservaMuestrasParaReintento` fuerza un fallo al hacer inaccesible temporalmente la carpeta de logs y verifica que el lote se escribe al recuperar la ruta. La compilación y el conjunto de pruebas deben ejecutarse después de los cambios.

Las pruebas automatizadas cubren el mutex, la cancelación solicitada durante el intervalo inicial y el límite/contador del búfer. La integración Alt+Tab usa estilos nativos de Windows: su funcionamiento Full/Mini ya fue comprobado manualmente por el usuario y la prueba automatizada no reemplaza futuras comprobaciones visuales.
