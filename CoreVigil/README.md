# CoreVigil

CoreVigil es un monitor ligero para Windows orientado a mostrar telemetria esencial del sistema en un HUD translucido, compacto y persistente.

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

## Arquitectura

El proyecto separa responsabilidades en las siguientes areas:
- Core/: contratos y modelos compartidos.
- Monitoring/: implementaciones que observan CPU, frecuencia, procesos, memoria y uptime.
- Services/: servicios de agrupacion, layout, persistencia, bandeja e inicio automatico.
- Diagnostics/: captura y escritura de telemetria interna de CoreVigil.
- ViewModels/: estado presentable y ciclo principal de monitorizacion.
- UI/: VigilUI y sus parciales para HUD, modos visuales, opacidad y persistencia.

Para mas detalle consulta docs/ARCHITECTURE.md.

## HUD

CoreVigil ofrece dos presentaciones que comparten el mismo nucleo de monitorizacion.

### Full

Presenta CPU, frecuencia, RAM, uptime y Actividad CPU con Top 8 de procesos. La actividad puede mostrarse agrupada por nombre o por instancias individuales.

### Mini

Presenta CPU y RAM en una capsula compacta. Incluye un LED dinamico ligado al nivel de CPU y conserva Topmost y translucidez porque sigue siendo la misma ventana VigilUI.

Las posiciones Full y Mini se guardan de forma independiente.

### Opacidad

La bandeja incluye la opcion `Opacidad...`. Abre un selector temporal con porcentaje y deslizador. El cambio se aplica en tiempo real al fondo de Full y Mini sin reducir la legibilidad de los controles y textos. El selector puede moverse desde su cabecera y conserva tanto la opacidad elegida como su posicion personalizada.

Consulta docs/HUD.md.

## Diagnostico interno

El diagnostico puede activarse desde la bandeja. Registra muestras de ejecucion en CSV para observar el coste propio de CoreVigil y detectar tendencias antes de optimizar.

Las pruebas prolongadas Full y Mini fueron completadas y analizadas. No mostraron evidencia clara de fugas ni degradacion progresiva. La estrategia vigente sigue siendo medir primero y optimizar solo cuando exista evidencia.

Consulta docs/DIAGNOSTICS.md y docs/TESTING.md.

## Documentacion

- docs/ARCHITECTURE.md: arquitectura y responsabilidades.
- docs/HUD.md: HUD Full, Mini, opacidad y persistencia visual.
- docs/DIAGNOSTICS.md: diagnostico interno y logs.
- docs/TESTING.md: validaciones realizadas y pruebas pendientes.
- docs/ROADMAP.md: prioridades futuras.
- docs/CHANGELOG.md: cambios notables bajo Keep a Changelog 1.1.0.

## Estado de validacion

El HUD Full y Mini, la persistencia independiente de posiciones, el cambio de modos, el Top 8 revisado, la ocultacion de Mini en la barra de tareas y el control persistente de opacidad han sido validados manualmente.

Las pruebas prolongadas Full y Mini tambien fueron completadas y analizadas. El siguiente frente de trabajo es Prioridad 1: observabilidad y pruebas automatizadas.
