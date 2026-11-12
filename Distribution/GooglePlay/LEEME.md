# HobbitRun - DBomb Studios

## Configuracion preparada

- Nombre visible: HobbitRun.
- Estudio: DBomb Studios.
- Application Identifier: `com.dbombstudios.hobbitrun`.
- Version: 1.0.0; Bundle Version Code: 1.
- Android minimo: API 26; Target API: 36 (Android 16).
- Backend IL2CPP y arquitectura ARM64.
- Build App Bundle activado en el perfil Android; Development Build desactivado.
- Escena inicial: Assets/Scenes/SampleScene.unity.
- Iconos Android: seis tamanos Legacy, seis Round y seis Adaptive con dos capas.
- Icono de tienda: `hobbitrun-play-icon-512.png`, PNG RGBA de 512 x 512.

## Crear la firma una sola vez

La cuenta de Play Console no crea automaticamente un archivo local de firma.
Para esta app nueva crea una clave de subida en Unity:

1. Abre Edit > Project Settings > Player. Selecciona Android y despliega Publishing Settings.
2. Pulsa Keystore Manager > Keystore > Create New > Anywhere.
3. Guarda `HobbitRun-upload.keystore` en una carpeta privada fuera del repositorio,
   por ejemplo Documents/DBombStudios/Signing. Evita Assets, Git y carpetas publicas.
4. Elige y confirma una contrasena segura para el archivo. Introducela solamente
   en Unity y guardala en tu gestor de contrasenas.
5. Crea una clave con Alias `hobbitrun-upload` y otra contrasena segura. Usa al
   menos 25 anos de validez y completa los datos de titular que solicita Unity
   con tus datos reales. No inventes pais, organizacion ni identidad.
6. Pulsa Add Key y acepta usar el archivo y la clave para el proyecto.
7. En Publishing Settings activa Custom Keystore si hace falta. Selecciona el
   archivo, su contrasena, el alias `hobbitrun-upload` y la contrasena de la clave.
8. Haz una copia de seguridad privada del keystore y conserva ambas contrasenas.
   Unity no conserva las contrasenas entre sesiones: vuelve a introducirlas al compilar.

No envies las contrasenas por chat ni subas el archivo de firma a Play Console
como si fuera un recurso grafico. El AAB se firma localmente con la clave de subida.
Al crear la primera version en Play Console, usa Play App Signing y permite que
Google genere/gestione la clave de firma de distribucion de esta app nueva.

## Exportar

1. Sal de Play y guarda la escena.
2. RunInfinity > Google Play > Comprobar configuracion.
3. Cuando la comprobacion este completa, RunInfinity > Google Play > Exportar AAB firmado.
4. Guarda `HobbitRun-1.0.0-1.aab` en `build/GooglePlay/`.

El exportador comprueba la configuracion y exige el keystore y las contrasenas
cargadas. No genera un AAB con la firma de depuracion para enviarlo a la tienda.
La compilacion final puede necesitar descargar dependencias de Gradle.
Todavia no se ha generado ni probado un AAB firmado: falta crear/cargar tu firma.
En actualizaciones futuras conserva el identificador y aumenta Bundle Version Code
(2, 3, ...). No recrees la clave para cada version.

## En Play Console

1. Crea la app HobbitRun (juego) en tu cuenta DBomb Studios.
2. Empieza con Prueba interna > Crear version. Configura Play App Signing y carga el AAB firmado.
3. Completa la ficha, icono, capturas reales del juego, grafico de funciones,
   contacto, politica de privacidad, clasificacion de contenido, publico objetivo,
   declaracion de anuncios y Seguridad de los datos segun el comportamiento real
   del juego y los SDK incluidos. No se han rellenado esas declaraciones desde aqui.
4. Revisa las advertencias de compatibilidad y el informe previo al lanzamiento,
   incluida la compatibilidad de bibliotecas nativas con paginas de 16 KB.
5. Instala la version desde el enlace de prueba y verifica celular y tablet.

Si tu cuenta es personal y fue creada despues del 13/11/2023, Google requiere
una prueba cerrada con al menos 12 verificadores durante 14 dias continuos antes
de solicitar acceso a produccion. La prueba interna no sustituye ese requisito.
El registro pagado no elimina los pasos que marque el panel de tu cuenta.

## Fuentes oficiales consultadas

- Target API actual: https://support.google.com/googleplay/android-developer/answer/11926878
- Firma en Unity: https://docs.unity.com/en-us/engine/6000.5/manual/platform-specific/android/getting-started/keystore/manager
- Play App Signing: https://support.google.com/googleplay/android-developer/answer/9842756
- Iconos y recursos: https://support.google.com/googleplay/android-developer/answer/9866151
- Pruebas de cuentas personales: https://support.google.com/googleplay/android-developer/answer/14151465
- Paginas de 16 KB: https://developer.android.com/guide/practices/page-sizes

## Arte

Los originales y las capas estan en Assets/Art/RunInfinity/AppIcons.
Se usaron image_gen integrado y la hoja del personaje como referencia visual.
`icon-prompts.txt` guarda los prompts exactos. El snippet reproducible
AgentScripts/ConfigureAppIcons.cs.txt exporta el tamano de tienda por muestreo
nearest-neighbor, crea la capa de fondo de color plano y asigna los iconos mediante
las APIs de Unity. No modifica el dibujo de los originales generados.
