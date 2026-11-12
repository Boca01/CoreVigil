# Roadmap de CoreVigil

## Principio rector

CoreVigil debe seguir siendo ligero, desacoplado y util como monitor permanente. Las nuevas funciones no deben convertir el HUD en un segundo Administrador de tareas completo.

## Estado actual

La base funcional incluye:
- monitorizacion esencial de CPU y RAM;
- frecuencia y uptime;
- Top 8 de actividad CPU;
- AGRUPAR / INDIVIDUAL;
- HUD Full;
- HUD Mini;
- Topmost y translucidez;
- control de opacidad persistente desde la bandeja;
- posicion persistente del selector de opacidad;
- persistencia visual;
- bandeja y autoinicio;
- instancia unica;
- diagnostico interno;
- proyecto `CoreVigil.Tests` con linea base de 26 pruebas automatizadas.

## Prioridad 0 - Validacion principal completada

Estado: COMPLETADA.

- Prueba prolongada Full completada y analizada.
- Prueba prolongada Mini completada y analizada.
- CPU propia, memoria, heap e intervalo revisados.
- Sin evidencia clara de fugas ni degradacion progresiva.
- Full y Mini considerados estables con la evidencia disponible.
- Mantener el principio de no optimizar sin evidencia.

## Prioridad 1 - Observabilidad y pruebas automatizadas

Estado: EN PROGRESO.

Completado hasta el checkpoint actual:
- proyecto `CoreVigil.Tests` integrado a la solucion;
- 34/34 pruebas automatizadas en verde;
- cobertura inicial de ProcessGroupingService;
- cobertura inicial de HudSettingsService;
- cobertura de invariantes publicos de RuntimeDiagnosticsService;
- cobertura de escritura CSV, escape, retencion temporal y limite de tamaño de RuntimeLogWriter;
- conservacion del lote ante fallo de escritura y reintento posterior;
- limite y contador de descartes del buffer de muestras;
- prueba del propietario del mutex y cancelacion de ProcessMonitor;
- cálculo y fallback de frecuencia actual de CPU, incluida frecuencia turbo;
- rutas configurables para aislar pruebas de configuracion y logs reales.

Siguiente frente:
- auditar el siguiente componente por riesgo y valor antes de agregar nuevas pruebas;
- priorizar comportamiento determinista y regresiones con impacto funcional;
- evitar introducir abstracciones solamente para incrementar el contador de pruebas.

## Prioridad 2 - Robustez de procesos

- Observar casos donde procesos nuevos no tengan muestra anterior.
- Validar procesos protegidos o que desaparecen entre enumeracion y lectura.
- Comparar de nuevo con Administrador de tareas si aparecen discrepancias sostenidas.
- Mantener el Top 8 como resumen, no como listado completo del sistema.

## Prioridad 3 - Geometria y pantallas

- Validar varios monitores.
- Validar DPI distinto entre monitores.
- Validar cambio de monitor con Full y Mini.
- Revisar restauracion si una pantalla deja de existir.
- Revisar el comportamiento de posiciones persistentes del HUD y del selector de opacidad en configuraciones multimonitor.

## Prioridad 4 - Experiencia Mini

Solo despues de estabilidad medida:
- valorar control Topmost directamente desde Mini;
- valorar tooltip o informacion adicional sin aumentar ruido;
- mantener el modo como telemetria de un vistazo.

## Fuera de alcance inmediato

- Reemplazar el Administrador de tareas.
- Mostrar todos los procesos permanentemente.
- Agregar nuevas metricas sin una necesidad clara.
- Optimizar por intuicion sin diagnostico.
