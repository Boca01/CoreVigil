# Contribuir a CoreVigil

Gracias por tu interes en CoreVigil.

CoreVigil es un monitor ligero para Windows orientado a mostrar telemetria esencial del sistema en un HUD compacto, persistente y de bajo ruido visual. El principio rector del proyecto es mantener esa identidad: una contribucion debe resolver una necesidad concreta sin convertir CoreVigil en un segundo Administrador de tareas.

## Antes de contribuir

Antes de proponer cambios:

1. Revisa `README.md` para conocer el alcance actual del proyecto.
2. Revisa `docs/ARCHITECTURE.md` antes de modificar responsabilidades entre capas.
3. Revisa `docs/TESTING.md` para conocer la linea base de validacion.
4. Revisa `docs/ROADMAP.md` y `docs/CHANGELOG.md` para entender decisiones y trabajo ya realizado.
5. Para cambios funcionales relevantes, abre primero un issue describiendo el problema o necesidad que se pretende resolver.

## Principios de contribucion

- Mantener CoreVigil ligero, desacoplado y util como monitor permanente.
- No agregar metricas, controles o complejidad sin una necesidad clara.
- Preferir cambios pequenos, revisables y con una justificacion concreta.
- Medir antes de optimizar. No introducir optimizaciones basadas unicamente en intuicion.
- Mantener separadas las responsabilidades de `Core`, `Monitoring`, `Services`, `Diagnostics`, `ViewModels` y `UI`.
- Evitar trasladar logica de negocio o monitorizacion al code-behind de la interfaz cuando exista una capa adecuada para ella.
- Mantener el modo Mini como telemetria de un vistazo.
- Conservar el Top 8 como resumen y no convertirlo en un listado permanente de todos los procesos.

## Estilo de codigo

- Conserva los comentarios existentes cuando sigan siendo correctos.
- Si un comentario existente debe cambiar, procura que el cambio sea coherente con la modificacion funcional realizada.
- Los comentarios nuevos del proyecto deben escribirse en espanol con redaccion profesional.
- Los identificadores, nombres de APIs y otros terminos tecnicos pueden mantenerse en ingles cuando sea lo natural.
- Mantener habilitado el analisis de nulabilidad y no ocultar advertencias mediante supresiones salvo que exista una justificacion tecnica documentada.
- No incluir credenciales, tokens, rutas privadas, logs personales, archivos de publicacion ni configuraciones locales del desarrollador.

## Compilacion

CoreVigil utiliza .NET 10 para Windows y WPF.

Desde la raiz del repositorio:

```powershell
dotnet restore
dotnet build
```

Una contribucion debe compilar sin errores. Las advertencias nuevas deben corregirse o justificarse tecnicamente antes de solicitar integracion.

## Pruebas

Ejecuta la suite completa antes de abrir un pull request:

```powershell
dotnet test
```

La linea base validada al preparar la publicacion publica es de 43/43 pruebas correctas. Si una contribucion modifica comportamiento determinista, agrega o actualiza las pruebas aplicables.

No agregues pruebas unicamente para aumentar el contador. Prioriza regresiones reales, contratos estables y comportamiento determinista.

## Validacion manual

Cuando un cambio afecte HUD, bandeja, persistencia, modos Full/Mini, procesos o geometria, documenta las validaciones manuales relevantes realizadas.

La validacion multimonitor y DPI mixto entre pantallas permanece como validacion futura del proyecto. No declares esos escenarios como validados sin evidencia reproducible.

## Pull requests

Un pull request debe:

- explicar que problema resuelve;
- describir el cambio realizado;
- indicar las pruebas automatizadas ejecutadas;
- indicar cualquier validacion manual relevante;
- mantenerse enfocado en un objetivo concreto;
- evitar incluir archivos generados, binarios, logs o configuracion personal;
- actualizar la documentacion cuando cambie el comportamiento publico o una decision tecnica relevante.

Los cambios notables deben registrarse en `docs/CHANGELOG.md`, manteniendo el formato Keep a Changelog 1.1.0 utilizado por el proyecto.

## Licencia y contribuciones

CoreVigil se prepara para distribucion bajo GNU GPLv3. Al enviar una contribucion aceptas que tu contribucion pueda distribuirse como parte de CoreVigil bajo la licencia del proyecto aplicable a esa version.

No envies codigo, recursos graficos, documentacion u otros materiales sobre los que no tengas derecho para contribuir bajo esas condiciones.

Las dependencias externas deben tener una licencia compatible con la distribucion del proyecto y deben documentarse cuando corresponda en `THIRD-PARTY-NOTICES.md`.

## Seguridad

No publiques vulnerabilidades de seguridad con detalles explotables en issues, discusiones o pull requests publicos. Sigue las instrucciones de `SECURITY.md`.

Gracias por ayudar a mejorar CoreVigil manteniendo su objetivo original y su simplicidad.
