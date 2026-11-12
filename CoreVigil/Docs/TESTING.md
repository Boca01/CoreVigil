# Pruebas de CoreVigil

## Estrategia actual

CoreVigil ha completado la validacion funcional y prolongada principal de los modos Full y Mini. La Prioridad 1 ya incluye una primera barrera automatizada de regresion sobre logica de procesos, persistencia y diagnostico interno.

La validacion manual no se considera equivalente a una suite automatizada. Ambos niveles se mantienen como evidencia complementaria.

## Linea base automatizada

Estado actual validado: 34 de 34 pruebas correctas.

Las pruebas viven en el proyecto `CoreVigil.Tests` y actualmente se organizan en siete suites:

### ProcessGroupingServiceTests - 8 pruebas

Protege:
- lista vacia;
- argumento nulo;
- agrupacion de instancias por nombre;
- comparacion de nombres sin distinguir mayusculas/minusculas;
- suma del consumo de CPU por grupo;
- orden de instancias por CPU descendente;
- desempate de instancias por ProcessId;
- orden de grupos por CPU total y desempate por nombre.

### HudSettingsServiceTests - 6 pruebas

Protege:
- carga sin archivo existente;
- ciclo Save/Load del estado completo;
- valores nulos para posiciones Mini y selector de opacidad;
- JSON invalido sin detener la aplicacion;
- compatibilidad con configuraciones historicas Left/Top;
- conservacion de opacidad, offsets y estado existente durante guardados parciales.

Las pruebas utilizan rutas temporales aisladas. El servicio mantiene el constructor predeterminado de produccion y dispone de una ruta configurable para pruebas.

### RuntimeDiagnosticsServiceTests - 4 pruebas

Protege invariantes publicos y estables:
- primera muestra con intervalo y CPU propia en cero;
- conservacion de los valores recibidos en RuntimeSample;
- CPU propia dentro del rango de 0 a 100 %;
- coherencia de IsManagedHeapReadingValid con ManagedHeapBytes.

No se exigen valores exactos dependientes de carga, reloj o hardware.

### RuntimeLogWriterTests - 10 pruebas

Protege:
- FlushAsync sin muestras no crea archivo;
- escritura de encabezado y datos;
- conservación de muestras y reintento después de un error de escritura;
- multiples flush sin duplicar encabezado;
- formato numerico invariante;
- escape CSV de TrayState;
- eliminacion de logs con mas de 7 dias;
- conservacion de logs recientes;
- limite agregado de 25 MB eliminando primero el log mas antiguo.
- limite de 10.000 muestras en memoria y contador de descartes de las mas antiguas.

Las pruebas se ejecutan sobre carpetas temporales y no utilizan los logs reales de CoreVigil.

### SingleInstanceGuardTests - 1 prueba

Protege la propiedad exclusiva del mutex, la salida de la segunda instancia sin liberación indebida y la adquisición posterior al cierre del propietario.

### ProcessMonitorTests - 1 prueba

Protege la cancelación del monitor durante el intervalo de muestreo inicial.

### CpuFrequencyMonitorTests - 4 pruebas

Protege:
- uso del `CurrentMhz` de Windows cuando PDH todavía no tiene una muestra válida;
- cálculo de frecuencia a partir del porcentaje de rendimiento PDH;
- conservación de frecuencias turbo superiores al máximo nominal;
- fallback cuando PDH entrega un valor no finito.

## Validaciones manuales realizadas

### Ventana y ciclo de vida

- La aplicacion permanece residente en bandeja al ocultar la ventana.
- La salida explicita cierra el proceso.
- La instancia unica ha sido validada: una segunda ejecucion no abre otro CoreVigil.
- El usuario verifico que Full aparece en Alt+Tab y Mini no aparece; ambos modos siguen accesibles según corresponda.

### HUD Full

- CPU global visible y actualizable.
- RAM visible y actualizable.
- Frecuencia visible.
- Uptime de sistema y aplicacion.
- Actividad CPU expandible/contraible.
- Conservacion del borde inferior al cambiar la altura.
- Topmost.
- Translucidez.

### HUD Mini

- CPU y RAM visibles.
- LED dinamico.
- Regreso a Full.
- Topmost heredado de la ventana.
- Translucidez.
- Movimiento independiente.
- Restauracion de posicion independiente.
- El modo Mini no aparece en la barra de tareas y continua accesible desde la bandeja.

### Opacidad

- La opcion `Opacidad...` esta disponible desde la bandeja.
- El selector aparece de forma temporal.
- El deslizador modifica la opacidad de fondo en tiempo real.
- Full y Mini comparten el valor configurado.
- El rango funcional utilizado es de 30 % a 100 %.
- Un clic fuera cierra el selector.
- La cabecera permite mover el selector sin interferir con el deslizador.
- La opacidad persiste despues de reiniciar CoreVigil.
- La posicion personalizada del selector persiste despues de reiniciar CoreVigil.

### Persistencia

- Full y Mini conservan posiciones diferentes durante la ejecucion.
- Estado persistente ampliado para Full/Mini.
- Estado de expansion de Actividad CPU persistente.
- Topmost persistente.
- Modo Mini/Full persistente.
- Opacidad de fondo persistente.
- Offsets del selector de opacidad persistentes.

### Procesos

- Vista INDIVIDUAL.
- Vista AGRUPAR.
- Grupos desplegables.
- PID por instancia.
- Top 8 limitado visualmente.
- Top 8 corregido para medir primero todas las instancias disponibles y aplicar despues el limite individual o agrupado.
- Comparacion visual con Administrador de tareas realizada durante actividad real.

## Pruebas prolongadas completadas

### Full

Sesion: `runtime-20261109-115635.csv`.

- Duracion aproximada: 2 h 06 min.
- 7455 muestras.
- CPU propia promedio: aproximadamente 0.224 %.
- P95: aproximadamente 0.519 %.
- P99: aproximadamente 0.766 %.
- Intervalo medio: aproximadamente 1014.34 ms.
- Working Set promedio: aproximadamente 179.58 MB.
- Heap administrado promedio: aproximadamente 9.06 MB.
- Sin evidencia de crecimiento progresivo.

### Mini

Sesion: `runtime-20261110-075548.csv`.

- Duracion aproximada: 2 h 48 min.
- 9940 muestras.
- CPU propia promedio: aproximadamente 0.168 %.
- P95: aproximadamente 0.390 %.
- P99: aproximadamente 0.646 %.
- Intervalo promedio: aproximadamente 1012.83 ms.
- Working Set promedio: aproximadamente 164.53 MB.
- Heap administrado promedio: aproximadamente 9.25 MB.
- Sin evidencia de crecimiento progresivo.
- Pico aislado de intervalo de aproximadamente 5523 ms.
- Dos lecturas invalidas de heap fueron correctamente identificadas con `IsManagedHeapReadingValid=False`.

## Dictamen de estabilidad

Full y Mini son estables con la evidencia disponible. No existe evidencia clara de fugas ni degradacion progresiva en las sesiones analizadas.

La Prioridad 0 queda tecnicamente completada y la Prioridad 1 dispone ya de una linea base de 26 pruebas automatizadas.

## Siguientes pruebas recomendadas

- Auditar el siguiente componente por riesgo y valor antes de ampliar la suite.
- Convertir nuevos hallazgos diagnosticos en criterios de regresion solo cuando sean deterministas.
- Validar procesos protegidos y procesos que nacen o terminan entre muestras.
- Validacion multimonitor y DPI mixto.
- Cambio de resolucion/escala mientras CoreVigil esta abierto.
- Restauracion tras cambios de disposicion de monitores.
- Repetir sesiones prolongadas solo si aparece nueva evidencia que lo justifique.

## Criterio de estabilidad

Una caracteristica puede considerarse estable cuando:
- compila sin errores;
- su comportamiento objetivo se reproduce consistentemente;
- no rompe Full, Mini, bandeja o persistencia;
- las pruebas automatizadas aplicables permanecen en verde;
- las pruebas prolongadas relevantes no muestran degradacion atribuible al cambio.
