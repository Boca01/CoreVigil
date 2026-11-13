# CoreVigil

CoreVigil es un monitor ligero para Windows orientado a mostrar telemetria esencial del sistema en un HUD translucido, compacto y persistente.

## Estado del proyecto

CoreVigil ha completado su alcance funcional actual y se mantiene en modo mantenimiento.

La linea base validada es de 43/43 pruebas automatizadas en verde, con compilacion final de 0 errores y 0 advertencias.

Las contribuciones son bienvenidas cuando resuelvan una necesidad concreta y mantengan el principio del proyecto: ofrecer telemetria esencial de forma ligera, desacoplada y sin convertir el HUD en un segundo Administrador de tareas.

## Estado actual

CoreVigil dispone actualmente de:
- Monitorizacion global de CPU.
- Frecuencia dinamica estimada del procesador.
- Uso de RAM.
- Uptime del sistema y de la aplicacion.
- Top 8 de procesos por consumo reciente de CPU.
- Vista de procesos INDIVIDUAL y AGRUPAR por nombre exacto.
- HUD completo translucido y adaptable.
- Modo Mini con CPU, RAM y LED dinamico de estado.
- Topmost para mantener el HUD sobre otras ventanas.
- Persistencia del estado visual y posiciones independientes de Full/Mini.
- Control de opacidad compartido para Full y Mini desde la bandeja.
- Selector de opacidad temporal, arrastrable y con posicion persistente.
- Integracion con bandeja del sistema.
- Iconos de bandeja dinamicos segun carga de CPU.
- Inicio automatico configurable con Windows.
- Instancia unica de la aplicacion.
- Diagnostico interno con sesiones CSV.
- Proyecto de pruebas automatizadas con linea base de 43/43 pruebas en verde.

## Arquitectura

El proyecto separa responsabilidades en las siguientes areas:
- Core/: contratos y modelos compartidos.
- Monitoring/: implementaciones que observan CPU, frecuencia, procesos, memoria y uptime.
- Services/: servicios de agrupacion, layout, persistencia, bandeja e inicio automatico.
- Diagnostics/: captura y escritura de telemetria interna de CoreVigil.
- ViewModels/: estado presentable y ciclo principal de monitorizacion.
- UI/: VigilUI y sus parciales para HUD, modos visuales, opacidad y persistencia.
- CoreVigil.Tests/: pruebas automatizadas de logica determinista y contratos estables.

Para mas detalle consulta docs/ARCHITECTURE.md.

## HUD

CoreVigil ofrece dos presentaciones que comparten el mismo nucleo de monitorizacion.

### Full

Presenta CPU, frecuencia, RAM, uptime y Actividad CPU con Top 8 de procesos. La actividad puede mostrarse agrupada por nombre o por instancias individuales.

### Mini

Presenta CPU y RAM en una capsula compacta. Incluye un LED dinamico ligado al nivel de CPU y conserva Topmost y translucidez porque sigue siendo la misma ventana VigilUI.

Las posiciones Full y Mini se guardan de forma independiente.

### Opacidad

La bandeja incluye la opcion `Opacidad...`. Abre un selector temporal con porcentaje y deslizador. El cambio se aplica en tiempo real al fondo de Full y Mini sin reducir la legibilidad de controles y textos. El selector puede moverse desde su cabecera y conserva tanto la opacidad elegida como su posicion personalizada.

Consulta docs/HUD.md.

## Diagnostico interno

El diagnostico puede activarse desde la bandeja. Registra muestras de ejecucion en CSV para observar el coste propio de CoreVigil y detectar tendencias antes de optimizar.

Las pruebas prolongadas Full y Mini fueron completadas y analizadas. No mostraron evidencia clara de fugas ni degradacion progresiva. La estrategia vigente sigue siendo medir primero y optimizar solo cuando exista evidencia.

## Pruebas automatizadas

La linea base actual es de 43/43 pruebas correctas:
- 8 de agrupacion de procesos;
- 6 de persistencia de preferencias del HUD;
- 4 de invariantes del diagnostico runtime;
- 10 de escritura CSV, reintento, limite de buffer y retencion de logs;
- 1 de propiedad del mutex de instancia unica;
- 6 de cancelacion y calculo de CPU del monitor de procesos;
- 4 del calculo y fallback de frecuencia de CPU.
- 4 del calculo de uso global de CPU.

Las pruebas de archivos utilizan rutas temporales para no modificar la configuracion ni los logs reales de CoreVigil.

Consulta docs/TESTING.md.

## Compilacion y pruebas

Requisitos principales:

- Windows.
- .NET 10 SDK.
- Entorno compatible con proyectos WPF para Windows.

Desde la raiz del repositorio:

dotnet restore

dotnet build

dotnet test

La linea base validada al preparar la publicacion publica es de 43/43 pruebas correctas.

## Documentacion

- docs/ARCHITECTURE.md: arquitectura y responsabilidades.
- docs/HUD.md: HUD Full, Mini, opacidad y persistencia visual.
- docs/DIAGNOSTICS.md: diagnostico interno y logs.
- docs/TESTING.md: validaciones manuales, prolongadas y automatizadas.
- docs/MEJORAS_Y_DECISIONES.md: motivo de las mejoras, mutex, cancelacion, diagnostico y Alt+Tab.
- docs/ROADMAP.md: prioridades futuras.
- docs/CHANGELOG.md: cambios notables bajo Keep a Changelog 1.1.0.

## Contribuciones

Las contribuciones son bienvenidas.

Antes de proponer cambios, consulta CONTRIBUTING.md para conocer los criterios de arquitectura, pruebas, documentacion y alcance del proyecto.

Los problemas de seguridad no deben publicarse con detalles explotables en issues o pull requests publicos. Consulta SECURITY.md.

## Estado de validacion

El HUD Full y Mini, la persistencia independiente de posiciones, el cambio de modos, el Top 8 revisado, la ocultacion de Mini en la barra de tareas y el control persistente de opacidad han sido validados manualmente.

Las pruebas prolongadas Full y Mini tambien fueron completadas y analizadas. Full aparece y Mini queda excluido de Alt+Tab segun la validacion manual del usuario. La Prioridad 1 de observabilidad y pruebas automatizadas queda completada con una linea base de 38/38 pruebas en verde.

La Prioridad 2 de robustez de procesos queda completada y la linea base global actual es de 43/43 pruebas en verde.

La validacion de geometria y pantallas fue completada en el entorno disponible con un solo monitor, incluyendo cambios de escala/DPI, funcionamiento de Full y Mini y restauracion despues de reiniciar. Las pruebas reales con varios monitores, DPI mixto entre pantallas y desconexion de una pantalla secundaria quedan pendientes por limitacion del entorno y no se consideran un defecto funcional observado.

La experiencia Mini se conserva deliberadamente como telemetria de un vistazo. No se agregan nuevos controles ni informacion adicional en el alcance actual salvo que aparezca una necesidad funcional demostrada.

## Licencia

CoreVigil se distribuye bajo GNU General Public License v3.0 (GPLv3).

Consulta LICENSE para los terminos completos y THIRD-PARTY-NOTICES.md para las dependencias y avisos de terceros identificados durante la preparacion del repositorio publico.
