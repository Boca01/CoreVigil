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
- persistencia visual;
- bandeja y autoinicio;
- instancia unica;
- diagnostico interno.

## Prioridad 0 - Cerrar validacion actual

- Completar 2 horas Full.
- Completar 2 horas Mini.
- Analizar CSV resultantes.
- Comparar CPU propia, memoria e intervalo.
- Documentar resultados reales.
- No optimizar sin evidencia.

## Prioridad 1 - Observabilidad y pruebas

- Convertir hallazgos del diagnostico en criterios de regresion.
- Agregar pruebas automatizadas donde la logica sea independiente de WPF/Windows.
- Priorizar agrupacion de procesos, persistencia y transformaciones de datos.

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
