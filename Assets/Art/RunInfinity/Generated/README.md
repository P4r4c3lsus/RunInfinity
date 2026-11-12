# RunInfinity: hojas de sprites v1

Generadas con la herramienta integrada image_gen, con fondo transparente.
Integradas en `Assets/Scenes/SampleScene.unity`. Los recortes se guardan en
los importadores de Unity; las hojas PNG originales se conservan completas.

## Personaje

`hobbit-sheet-v1.png`: cuadrícula 4 x 4.
- Fila 1: cuatro poses de reposo.
- Filas 2 y 3: ocho poses de carrera.
- Fila 4: anticipación, ascenso, salto y aterrizaje.

## Obstáculos

`spikes-rock-vines-sheet-v1.png`: cuadrícula 4 x 2.
- Fila 1: cuatro variantes de picos de roca.
- Fila 2: cuatro variantes con enredaderas.

## Uso en Unity

Ya configuradas como Sprite (2D and UI), Multiple, Point, sin compresión ni
mipmaps. Se recortaron 16 sprites Hobbit y 8 Spike por transparencia, con pivote
Bottom Center. La escena conserva los colliders y el movimiento originales.
`Runner/HobbitVisual` usa el Animator `../Animations/Hobbit.controller`.
RICharacterAnimation selecciona Idle, Run, Jump, Fall y Land según el juego y
el Rigidbody2D; respeta pausa, derrota y parpadeo de invulnerabilidad.
Los hijos RockSpikeVisual de los obstáculos seleccionan una de ocho variantes
al activarse. Los renderers originales quedan ocultos como respaldo.
Las siluetas triangulares son una interpretación; las referencias de sprites
de la escena no se encontraron entre los archivos fuente del proyecto.

Los prompts completos están en `generation-prompts.txt`.

## Reproducir la instalación o la prueba

El snippet `AgentScripts/IntegrateSprites.cs.txt` se ejecuta mediante el comando
eval de Unity Pipeline con la escena abierta fuera de Play. Crea o actualiza los
recortes, clips, controlador e hijos visuales y guarda la escena. No editar a mano
los metadatos de recorte. El proyecto incluye com.unity.pipeline para esta conexión.

`AgentScripts/VerifySpriteIntegration.cs` es una prueba de integración ejecutable
mediante script run durante Play. Comprueba avance de frames, cambios de sprite,
Run/Jump/Fall/Land, congelación en pausa y variantes en obstáculos instanciados.
Deja la prueba pausada para inspección visual; salir de Play al terminar.
