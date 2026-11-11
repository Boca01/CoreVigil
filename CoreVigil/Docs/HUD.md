# HUD de CoreVigil

## Proposito

El HUD permite observar el estado esencial del equipo sin depender continuamente del Administrador de tareas. La interfaz esta pensada para permanecer visible de forma discreta y ofrecer una vista completa o una vista minima.

## VigilUI

VigilUI es la ventana WPF vigente.

Caracteristicas de ventana:
- sin marco convencional;
- fondo transparente;
- contenido translucido;
- no redimensionable por el usuario;
- movimiento mediante arrastre;
- Topmost configurable;
- persistencia de estado;
- modos Full y Mini dentro de la misma ventana.

## Modo Full

El modo Full muestra:
- CPU global.
- Frecuencia dinamica estimada.
- RAM usada/total y porcentaje.
- Uptime del sistema.
- Uptime de CoreVigil.
- Actividad CPU.
- Top 8.
- Modos AGRUPAR e INDIVIDUAL.

### Actividad CPU

La seccion puede expandirse o contraerse.

El cambio de altura conserva el borde inferior cuando el HUD esta colocado junto a la barra de tareas, evitando que la ventana se desplace progresivamente con cada ciclo.

### Procesos

#### INDIVIDUAL

Ordena instancias individuales por consumo reciente y muestra las ocho primeras.

#### AGRUPAR

Agrupa por nombre exacto, suma el consumo de las instancias y despues limita la vista a los ocho grupos principales.

Los grupos son desplegables para consultar las instancias y PID que los componen.

## Modo Mini

El modo Mini es una capsula horizontal compacta que presenta:
- RAM %.
- CPU %.
- LED dinamico de CPU.
- control para regresar al HUD Full.

No crea otro monitor. Reutiliza el mismo DataContext y el mismo ciclo de adquisicion que Full.

En Mini, CoreVigil no necesita aparecer en la barra de tareas. El acceso continua disponible desde la bandeja del sistema y al regresar a Full se restaura el comportamiento normal de la ventana.

### LED dinamico

El LED utiliza cuatro estados visuales alineados con los niveles utilizados por el icono de bandeja:
- Low: CPU menor a 40 %.
- Medium: CPU menor a 70 %.
- High: CPU menor a 90 %.
- Critical: CPU igual o mayor a 90 %.

El LED utiliza color y glow para funcionar como indicador de un vistazo.

## Topmost

Topmost pertenece a la ventana VigilUI, por lo que continua vigente al cambiar entre Full y Mini.

El estado se persiste localmente.

## Opacidad y translucidez

La ventana utiliza transparencia WPF. La opacidad controlada corresponde al fondo visual del HUD y no a `Window.Opacity`, por lo que textos, numeros, LED y controles mantienen su legibilidad.

Full y Mini comparten el mismo valor configurable de opacidad.

### Selector de opacidad

La bandeja del sistema incluye la opcion `Opacidad...`.

Al activarla:
- se muestra un panel temporal;
- se presenta el porcentaje actual;
- el deslizador utiliza un rango de 30 % a 100 %;
- los cambios se reflejan en tiempo real en Full y Mini;
- un clic fuera del selector lo cierra.

El selector aparece asociado al HUD. Su cabecera `OPACIDAD / porcentaje` funciona como zona de arrastre, mientras que el deslizador conserva exclusivamente la funcion de modificar la opacidad.

Si el usuario mueve el selector, sus offsets horizontal y vertical se conservan. Si no existe una posicion personalizada, se utiliza la posicion automatica asociada al HUD.

## Persistencia

La configuracion visual guarda:
- posicion historica Left/Top para compatibilidad;
- FullLeft;
- FullTop;
- MiniLeft;
- MiniTop;
- Topmost;
- IsCpuActivityExpanded;
- IsMiniMode;
- BackgroundOpacity;
- OpacityPopupHorizontalOffset;
- OpacityPopupVerticalOffset.

Esto permite que:
- Full recuerde su posicion;
- Mini recuerde otra posicion;
- mover Mini no desplace permanentemente Full;
- la aplicacion restaure el modo utilizado anteriormente;
- la opacidad elegida se conserve despues de reiniciar;
- el selector de opacidad pueda reaparecer en la posicion personalizada elegida.

## Perfiles de layout

HudLayoutService selecciona perfiles visuales en funcion del area util y DPI.

Los perfiles Compact, Standard y Spacious pertenecen al layout del HUD Full. El modo Mini es una decision de presentacion distinta y no debe confundirse con esos perfiles.

## Restriccion actual de geometria

La logica vigente utiliza el area de trabajo de Windows para mantener el HUD visible. El soporte multimonitor debe seguir tratandose con cuidado antes de considerarse completamente validado en configuraciones complejas.
