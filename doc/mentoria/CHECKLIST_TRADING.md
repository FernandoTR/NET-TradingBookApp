# Checklist de trading fundamentado en las transcripciones

**Fecha de revisión:** 22 de octubre de 2026.  
**Corpus:** los 23 archivos `.md` de `transcripciones`, revisados íntegramente mediante búsqueda recursiva; 14 234 líneas. Hay sesiones 1–4 y 6–24; no existe un archivo de la sesión 5 en el directorio. El inventario completo figura en D.  
**Alcance:** especificación documental para evaluar setups en TradingView. No se ha desarrollado ni probado un indicador en Pine Script.

## A. Resumen de la estrategia

La metodología construye un **mapa del precio** a partir de rangos, movimientos, impulsos, checkpoints, cajas, pivotes y líneas de trabajo. Después busca una reacción o continuación reconocible en una de esas zonas y comprueba que exista espacio suficiente hasta el siguiente obstáculo para justificar el riesgo técnico.

El núcleo se formula expresamente como **tres requisitos simultáneos: zona, gatillo y ratio mínimo 1:1**. Se repite en las sesiones 10, 11, 12, 16, 17, 18, 19, 23 y 24. Por eso se seleccionan tres condiciones, sin añadir una cuarta de tendencia o divergencia que el material no impone universalmente. Véanse Z1, G1 y R1 en D.

La **etapa 1** obtiene líneas del cambio de estructura y mantiene su vigencia mediante la regla 1234. La **etapa 2** obtiene zonas temporales de figuras y contexto; está subordinada a la etapa 1. No es obligatorio que exista una figura de etapa 2 para que un setup de etapa 1 sea válido. La sesión 24 incluso admite especializarse inicialmente en etapa 1.

Los gatillos básicos son alternativas, no confirmaciones acumulativas:

- **Reversión:** engulfing o envolvente, stars, pinzas y fake out.
- **Continuación:** pullback perfecto, largo o corto; su secuencia es rompimiento → retesteo → continuación.

La regla docente inicial exige cerrar el patrón de reversión. Los pullbacks se activan al superar el extremo correspondiente una vez completada su secuencia, sin exigir cierre. La sesión 23 permite además anticipar una reversión cerca del cierre: es una diferencia operativa que debe resolverse expresamente antes de automatizar.

### Conceptos necesarios para interpretar el checklist

| Término del material | Significado operativo identificado | Precaución para programar |
|---|---|---|
| Rango | Máximo–mínimo completo de una vela que aporta información; las velas interiores pueden ser ruido. | No equivale automáticamente a contar cada vela como movimiento. |
| Movimiento | Secuencia de rangos en una dirección; puede contener velas de distinto color. | La definición inicial por colores se refina después por rangos. |
| Impulso | Estructura direccional de máximos y mínimos más ruptura del checkpoint pertinente. | El primer impulso no puede exigir un checkpoint previo inexistente. |
| Checkpoint o check | Extremo contrario entre extremos consecutivos del movimiento; límite fijo de la caja. | Se diferencia de la línea dinámica que sigue el extremo favorable. |
| Caja | Check fijo y extremo opuesto dinámico. | Romper un rango no equivale a romper una caja ni a confirmar un impulso. |
| Pivote | Inicio de un nuevo impulso confirmado y final del anterior. | No es, por definición, el fractal estándar de un número fijo de velas. |
| Línea o zona de trabajo | Referencia del mapa donde buscar comportamiento de entrada. | Tiene influencia alrededor del trazo; no se proporciona una anchura universal. |
| Regla 1234 | Un pivote crea línea sin confirmar; dos sobre la misma línea la confirman; tres strikes eliminan la no confirmada y cuatro la confirmada. | Cuenta comportamiento en el frame de origen, no cuatro velas ni cruces de cualquier frame. |

### Qué tipo de afirmaciones contiene el material

**Reglas explícitas de setup:** las tres condiciones; adecuación del frame a la zona; requisitos propios de cada familia de gatillo; stop técnico; ratio medido al siguiente obstáculo. Una zona por sí sola no autoriza una entrada de trading.

**Recomendaciones o criterios de calidad:** preferir escenarios A/B, rechazos más claros, mayor convergencia, cerrar más lejos de la mitad del rango y colocar el TP algo antes de una zona. No se convierten aquí en filtros numéricos obligatorios.

**Ejemplos:** precios de Bitcoin, porcentajes de riesgo, capitales, apalancamientos, resultados de alumnos y repartos entre TPs. No establecen por sí mismos reglas generales ni prueban el rendimiento de la estrategia.

**Requisitos para operar en real que no se verifican sólo con OHLC:** el Trading Book o grimorio personal, la prueba previa y la gestión del capital. Las sesiones 21 y 24 exigen habilitar los gatillos con la estadística personal —se utiliza el umbral de 70% o más— y pasar por backtesting y paper trading. Ese porcentaje es un criterio docente de selección retrospectiva, no una probabilidad garantizada para la siguiente entrada. También existen restricciones operativas por mercado: en forex debe estar abierta al menos una sesión correspondiente a las divisas del par; en acciones, bolsa abierta y cierre de la operación intradía antes del cierre bursátil. La condición de sesión de forex no se exige a cripto. Además se pide legibilidad del gráfico y liquidez suficiente. Se documentan como requisitos externos al núcleo gráfico, sin inventar horarios actuales ni umbrales de liquidez.

## B. Checklist propuesto

Evaluar una **dirección, zona, familia de gatillo y momento concretos**. Un dato desconocido debe producir **Pendiente**, no una validación por defecto.

| Condición | Regla de validación | Datos necesarios | Automatización |
|---|---|---|---|
| **1. Zona de trabajo vigente y frame habilitado** | Existe una zona identificada con información disponible en ese momento, sigue vigente y admite el frame del gatillo. El comportamiento se produce respecto de esa zona conforme a su familia: rechazo, ruptura o retesteo. No basta un patrón en medio del gráfico sin referencia del mapa. | OHLC de los frames de análisis; checkpoints, pivotes, anclajes y límites de zona; etapa, frame de origen y fecha de creación/invalidación. | **Parcial:** selección y anchura de zona inicialmente manuales; vigencia y relaciones de precio calculables si se formalizan sus reglas. Evidencia Z1–Z5. |
| **2. Gatillo válido según su familia** | Se cumple **una** alternativa admitida, con dirección y relación con la zona correctas. En reversión se valida el patrón y, en etapa 1, la punta del movimiento; en continuación deben existir rompimiento, retesteo válido y superación del extremo de continuación. | OHLC ordenado; velas del patrón o extremos de las tres fases; zona; extremo del movimiento; frame y modalidad temporal. | **Parcial:** algunas desigualdades son automáticas; similitud, indecisión y ciertos límites requieren definición o confirmación manual. La operativa intravela tiene límites históricos. Evidencia G1–G6. |
| **3. Ratio mínimo 1:1 con stop técnico y camino disponible** | Entrada, SL y objetivo están definidos del lado correcto; beneficio potencial / pérdida potencial **≥ 1**, sin atravesar un obstáculo previo del mapa para fabricar el ratio. El SL respeta la invalidación del gatillo y la zona. | Entrada prevista, SL, objetivo, siguiente línea/check/zona relevante y momento de evaluación; costes si se decide incluirlos. | **Automática para el cálculo; parcial para el conjunto:** escoger zona de invalidación, márgenes y objetivo puede requerir intervención. Evidencia R1–R3. |

**Resultado:** sólo cuando las tres condiciones cumplen se obtiene un **setup gráfico válido dentro del perfil elegido**. Esto no acredita por sí solo la habilitación del gatillo para operar en real ni ejecuta una orden.

## C. Definición técnica

Se distinguen dos niveles en lo que sigue:

- **Regla del material:** criterio respaldado por las transcripciones.
- **Propuesta de implementación pendiente de validación:** traducción técnica sugerida donde faltan precisiones. No forma parte de la estrategia original hasta que se valide.

### C1. Zona de trabajo vigente y frame habilitado

**Descripción según las transcripciones.** Los gatillos sólo se buscan en líneas o zonas de trabajo. El mapa debe incluir los frames mayores; una reacción aislada no confirma automáticamente una línea. Las zonas tienen anchura y contexto, y sus límites no se reducen a un precio exacto.

**Información que debe quedar fijada antes de evaluar: características de la zona, etapa, anclajes, frame de origen, momento desde el que existe, estado y dirección del setup.** Si esos datos no están definidos, la condición permanece Pendiente.

**Regla de frame básica para líneas de etapa 1 y zonas/figuras de etapa 2:**

| Frame de origen de la línea | Frames de gatillo admitidos en el modelo básico |
|---|---|
| Semanas | Semanas o días |
| Días | Días o 4 horas |
| 4 horas | 4 horas o 1 hora |
| 1 hora | 1 hora o 15 minutos |
| 15 minutos | 15 o 5 minutos |
| 5 minutos | 5 o 1 minuto |
| 1 minuto | 1 minuto o 30 segundos, si los datos están disponibles |

Esta es una **secuencia concreta**, no la instrucción genérica de dividir cualquier temporalidad por un número. La sesión 15, línea 641, extiende a etapa 2 la posibilidad de trabajar en el frame donde se ve la figura y uno inferior. Las excepciones por convergencia de frames son avanzadas; no se habilitan por defecto. La sesión 24 reafirma días → días/4 h para la fase básica.

**Vigencia de etapa 1:** identificar si la línea está sin confirmar o confirmada; aplicar strikes en su frame de origen; no usar eventos anteriores a su creación ni eliminar el pivote simplemente porque desaparezca una línea. **Un nuevo pivote válido en la zona reinicia los strikes a cero; una reacción aislada no los reinicia.** Una línea sin confirmar puede ser una zona operable: exigir siempre dos pivotes como condición de entrada añadiría una restricción ausente del material. La etapa 2 requiere controlar que la figura y su argumento sigan vigentes, sin aplicarle automáticamente 1234.

**Cumple:** la zona era conocida, está vigente y el frame es admisible; se puede identificar la relación del setup con ella. **No cumple:** no hay zona, ya se invalidó, el frame básico no está autorizado o el patrón no guarda relación con la referencia elegida. **Pendiente:** faltan anclajes, anchura, identificación estructural o resolución de un conflicto entre zonas.

**Propuesta de implementación pendiente de validación:** comenzar con zonas y anclajes seleccionados por el usuario, con límites de influencia explícitos. Para una interacción que requiera contacto, comparar el intervalo máximo–mínimo de las velas pertinentes con la franja de zona, en vez de exigir que el cierre permanezca dentro. Esta comparación no se debe aplicar como requisito de toque a un **pullback corto**, cuyo retesteo puede quedarse antes de la línea, ni confundirla con la ruptura obligatoria del fake out.

Si dos líneas encierran una zona de conflicto, evaluar la franja completa: superar sólo la primera no acredita haber salido del conflicto. La distancia disponible y el stop se miden teniendo en cuenta toda la zona.

**Parámetros:** símbolo y fuente de datos; frame de zona y de gatillo; etapa; precios/tiempos de anclaje; límites de influencia; estado de línea; contador de strikes cuando proceda. No se fija un porcentaje, ATR ni número de ticks para la anchura: las fuentes no aportan un valor universal.

**Limitación:** las líneas inclinadas y las proyecciones deben respetar el trabajo en escala logarítmica descrito por el curso. Antes de automatizarlas hay que acordar su geometría en precio y tiempo; un ángulo visual de pantalla no basta como regla numérica.

### C2. Gatillo válido según su familia

**Descripción según las transcripciones.** El gatillo representa comportamiento, no la sola llegada a un nivel ni la aparición de un indicador. Las cuatro reversiones y los tres pullbacks son alternativas. El patrón se interpreta en relación con el movimiento previo y con la zona.

| Alternativa | Parte verificable respaldada por el material | Lo que aún necesita definición |
|---|---|---|
| **Engulfing / envolvente** | Rechazo contrario al movimiento previo. En la versión bajista aceptada por intención, el cierre final queda por debajo del mínimo del rango izquierdo; en la alcista, por encima del máximo. La forma ideal envuelve el rango anterior, pero se admiten variantes que no lo encierran completamente por el lado de apertura. | Delimitar las variantes aceptadas y la penetración admisible respecto de la zona. No sustituir sin más esta definición por el patrón estándar de cuerpo a cuerpo. |
| **Stars** | Tres partes: hombro izquierdo, cabeza y hombro derecho. En bajista, el cierre derecho llega al menos a la mitad del rango high–low izquierdo y se prefiere por debajo; en alcista, simétrico. Los hombros no deben exceder el extremo de la cabeza. | Indecisión de la cabeza, variantes con varias velas e igualdad exacta en el 50%: aparece como límite aceptable y también como caso que el docente prefiere evitar. |
| **Pinzas** | Rangos iguales o similares, orden que produzca reversión y cierre favorable al rechazo. Un cierre más próximo al extremo favorable es preferible. | Tolerancia de similitud y mínimo rechazo aceptable. No hay porcentaje obligatorio de cuerpo o mecha. |
| **Fake out** | Debe existir ruptura de la referencia y rechazo de esa ruptura; no vale quedarse corto antes de romper. Se describe como rechazo que puede desarrollarse en una vela. | Elegir si «romper» significa atravesar el trazo o toda su franja y cuantificar los límites del rechazo. Una mecha mayor es calidad preferida, no un ratio mecha/cuerpo impuesto. |
| **Pullback perfecto** | Rompimiento → retesteo de la línea → continuación más allá del extremo del rompimiento. SL tras el extremo del retesteo, con margen técnico. | Tolerancia de «perfecto» y margen de SL. |
| **Pullback largo** | El retesteo atraviesa la línea, pero su punto medio debe permanecer al menos en el lado favorable del rompimiento. Si invalida esa condición, una nueva ruptura exige reconstruir retesteo y continuación. | Aplicar el punto medio al movimiento de retesteo, no inventar un Fibonacci del impulso completo; confirmar su medición en las variantes visuales. |
| **Pullback corto** | El retesteo no llega a la línea. La distancia desde su extremo a la línea no puede ser mayor que la amplitud del retroceso. Después debe superarse el extremo del rompimiento. | Qué significa quedar «muy cerca» para situar el SL detrás de la línea o a mitad del espacio. |

**Punta en reversión:** en etapa 1 no se persigue un segundo patrón que queda dentro del movimiento, sin volver al extremo anterior. El nuevo patrón debe alcanzar el extremo relevante o producir uno nuevo. La sesión 15 admite rebotes interiores de canal en etapa 2; por tanto, la punta del impulso mayor no se impone indiscriminadamente a todas las figuras y etapas.

**Límite transversal de reversión:** la sesión 17, líneas 251–258, exige que la mitad del rango pertinente quede a favor del rechazo y de la operación. No basta identificar la silueta de una pinza o envolvente. La referencia exacta de ese rango respecto de la zona y el tratamiento de la igualdad deben validarse para cada variante; si no están determinados, el gatillo queda Pendiente. Esta mitad no se confunde con la del hombro izquierdo de stars ni con un retroceso del impulso completo.

**Duración mínima del pullback rápido:** requiere al menos **dos velas del frame de gatillo**. Las tres fases dentro de una única vela de ese frame no lo habilitan por defecto, aunque se observe más detalle intrabar. La segunda vela puede continuar antes de cerrar; exigir dos velas no implica exigir dos cierres completos.

**Temporalidad de validación:**

- Reversión: las sesiones 10, 11 y 16 exigen el cierre. La sesión 23 permite y favorece operativamente la entrada un poco antes del cierre. Son modalidades distintas.
- Continuación: completar las tres fases y superar el extremo de continuación; no exigir por defecto un cierre posterior. Una mera ruptura no es un pullback completo.

**Propuesta de implementación pendiente de validación — perfil reproducible inicial:** confirmar las reversiones únicamente con velas cerradas; mantener los pullbacks intravela como Pendiente de confirmación manual o de evidencia intrabar ordenada. Si se decide esperar también al cierre para los pullbacks, etiquetar esa modalidad como una variante más tardía, no como reproducción literal de la entrada original. No mostrar retrospectivamente el precio ideal de una entrada que el algoritmo sólo pudo reconocer después.

**Cuantificaciones propuestas, no reglas nuevas:**

Las dos expresiones siguientes usan **distancias aritméticas de precio**. Deben contrastarse con la medición visual y logarítmica del curso; no se presume equivalencia automática entre ambas geometrías.

- Para el pullback largo alcista, si `B` es el extremo del rompimiento, `R` el mínimo del retesteo y `L` el nivel de la línea en el punto de evaluación, traducir el punto medio como `(B + R) / 2 ≥ L`; para el bajista, el máximo de retesteo y la desigualdad inversa. En líneas inclinadas hay que validar en qué instante se evalúa `L`. No equivale necesariamente a retroceder 50% del impulso anterior.
- Para el pullback corto alcista, con `B > R > L`, traducir la comparación visual como `R − L ≤ B − R`; para el bajista, `B < R < L` y `L − R ≤ R − B`. La tolerancia de la zona y el instante de `L` siguen pendientes.
- Para pinzas o indecisión, sería posible parametrizar similitud de rangos o tamaño relativo del cuerpo, pero **no se eligen valores**: no hay respaldo para atribuirles un porcentaje al curso.

**Cumple:** una alternativa tiene todos sus requisitos definidos, observados y disponibles en el momento correcto. **No cumple:** falla un requisito conocido —por ejemplo, fake out sin ruptura o pullback sin continuación—. **Pendiente:** patrón incompleto, vela aún abierta en el perfil de cierre, secuencia intravela desconocida o criterio cualitativo sin validar. La ausencia de una definición no se trata como aprobación.

**Parámetros:** familia y alternativa; dirección; etapa; extremo previo; velas/extremos que integran el patrón; modalidad cierre/intravela; límites de zona; tolerancias de las variantes que se validen. Los siete gatillos no deben ser siete requisitos simultáneos.

### C3. Ratio mínimo 1:1 con stop técnico y camino disponible

**Descripción según las transcripciones.** El recorrido hasta la siguiente línea debe justificar la distancia a la invalidación. No se ajusta el stop a un porcentaje de riesgo monetario deseado: primero se coloca por estructura y después se calcula el tamaño de posición.

Definir `E` = entrada prevista, `S` = SL técnico y `T` = objetivo alcanzable sin saltar el primer obstáculo relevante del mapa.

- **Largo:** `S < E < T`; riesgo por unidad = `E − S`; beneficio por unidad = `T − E`.
- **Corto:** `T < E < S`; riesgo por unidad = `S − E`; beneficio por unidad = `E − T`.
- **Validación:** beneficio por unidad / riesgo por unidad **≥ 1**, con riesgo estrictamente positivo.

Estas expresiones formalizan la medición 1:1 de la fuente, no añaden un filtro estadístico. Se evita usar valores absolutos que oculten un SL o un TP colocados del lado incorrecto.

**Selección del SL:** para reversiones, más allá del patrón o de la zona, el que quede más lejos de la entrada, con espacio técnico. Para pullbacks, seguir la ubicación específica de su variante; no aplicarles indiscriminadamente un SL genérico de reversión. La zona de conflicto puede exigir situarlo detrás de la franja completa. El curso utiliza márgenes aproximados y después pide calibrarlos por activo/frame; no se prescribe aquí un buffer universal.

**Selección de T:** primera referencia relevante en el sentido de la operación, incluidos checks que actúen como obstáculos. La sugerencia de cobrar algo antes de la zona es una recomendación; si se elige ese precio, el ratio se calcula con él. En etapa 2, una proyección lejana no elimina una línea previa de etapa 1: calcular hasta el primer freno operativo. Las proyecciones de figuras y sus excepciones no se generalizan a todo setup.

**Cumple:** E/S/T definidos, orden correcto, SL coherente y ratio ≥ 1 hasta el objetivo permitido. **No cumple:** ratio < 1, distancias nulas, orden incorrecto o necesidad de ignorar un obstáculo para llegar a 1R. **Pendiente:** faltan precios, margen, objetivo o resolución de la zona siguiente.

**Parámetros:** E/S/T o reglas validadas para obtenerlos; margen técnico; obstáculos activos; instante de evaluación. El mínimo de estrategia queda en 1. Un mínimo mayor configurado por el usuario sería un filtro adicional explícito.

**Propuesta de implementación pendiente de validación:** mostrar el ratio bruto de precio y, si el usuario incorpora costes, distinguirlo del ratio ajustado. No hay datos suficientes para fijar comisiones, spread o deslizamiento actuales. Para un patrón al cierre, el cierre puede servir como referencia de cálculo, sin presumir que la ejecución real se obtendrá a ese precio. Recalcular si la entrada cambia o si una línea inclinada reduce el espacio con el tiempo.

## D. Evidencia documental

Las referencias corresponden a **líneas físicas de los archivos revisados**, no a minutos del vídeo. Los fragmentos conservan la transcripción, incluso cuando presenta errores de reconocimiento. Se enlaza la línea inicial de cada pasaje; los intervalos indicados permiten leer su contexto.

### Evidencia de las condiciones

| ID | Fuente y líneas | Fragmento y alcance |
|---|---|---|
| **Z1** | [10 PM - Regla multiframe, Engulfing.md, 126–138](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/10 PM - Regla multiframe, Engulfing.md:126>) | «requerimientos forzosos»; «los gatillos […] sólo van a ser válidos […] en una línea o zona de trabajo»; «ratio mínimo […] 1 a 1». Obligación explícita de las tres condiciones. |
| **Z2** | [10 PM - Regla multiframe, Engulfing.md, 316–321](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/10 PM - Regla multiframe, Engulfing.md:316>), [15 PM - Replay.md, 641](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/15 PM - Replay.md:641>) y [24 PM.md, 425–429](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/24 PM.md:425>) | «semanas y […] días»; «días y 4 horas» y secuencia posterior. En 15: «se puede trabajar en el frame donde se ve y 1 inferior también» para etapa 2. La última sesión reafirma el modelo básico y separa la convergencia avanzada. |
| **Z3** | [8 PM - 1234.md, 358–400 y 463–480](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/8 PM - 1234.md:358>) | Dos pivotes confirman; «3 strikes» sin confirmar y «4» confirmada. Se cuenta en el frame de creación y sólo en etapa 1. Una reacción no sustituye un pivote confirmado. |
| **Z4** | [9 PM - Contexto.md, 458–486](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/9 PM - Contexto.md:458>) | Zona encerrada entre líneas; hay que superar la zona entera y situar el stop al otro lado. No operar ese conflicto en su propio frame es recomendación contextual; se admite bajar si existe espacio. |
| **Z5** | [9 PM - Contexto.md, 278–282 y 423–426](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/9 PM - Contexto.md:278>) | «ese pivote borra todos los strikes»; «una reacción no las quita tiene que ser un pivote». Reinicio a cero sólo cuando el pivote en la zona queda confirmado. |
| **G1** | [11 PM - Gatillos.md, 368–370](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/11 PM - Gatillos.md:368>) y [16 PM.md, 22–27](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/16 PM.md:22>) | «si no hay línea no se vale buscar patrones»; «tiene que haber un gatillo»; «al menos […] uno a uno». En 16 se llaman condiciones «inalterables». |
| **G2** | [10 PM - Regla multiframe, Engulfing.md, 236–252](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/10 PM - Regla multiframe, Engulfing.md:236>) y [11 PM - Gatillos.md, 63–86](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/11 PM - Gatillos.md:63>) | Forma ideal de envolvente y variante por intención: «siempre y cuando el cierre del movimiento termine por debajo del rango izquierdo». No exige siempre la envoltura ideal completa. |
| **G3** | [11 PM - Gatillos.md, 129–177, 182–214 y 229–251](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/11 PM - Gatillos.md:129>) | Stars: «mitad del rango de la vela izquierda»; pinzas: «rangos […] similares o iguales»; fake out: «si no hay un rompimiento no puede ser un falso rompimiento». Similitud e indecisión no tienen tolerancia numérica. |
| **G4** | [11 PM - Gatillos.md, 265–281, 283–355 y 534–536](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/11 PM - Gatillos.md:265>) | Reversión «en la punta»; secuencia rompimiento → retesteo → continuación; «aquí no esperamos al cierre»; largo «mitad del […] retesteo […] a favor»; corto espacio «igual o menor a la caja». Rápido: «mínimo lo que tienes que necesitar son dos velas». |
| **G5** | [15 PM - Replay.md, 214–234 y 692–710](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/15 PM - Replay.md:214>) y [23 PM - Operativa.md, 401–427 y 441–454](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/23 PM - Operativa.md:401>) | Rebotes interiores de etapa 2 matizan la punta. En 23: «sí o si necesito esperar a un cierre» y después «no están prohibido que gatillan antes de que cierre»; el pullback «se opera a la zona». Diferencia temporal que no debe ocultarse. |
| **G6** | [17 PM.md, 251–258](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/17 PM.md:251>) | «los patrones de reversión la mitad del rango tiene que estar a favor» del rechazo; «el 50% tiene que estar a favor de la operación». El gráfico ausente impide fijar sin validación todos sus anclajes. |
| **R1** | [10 PM - Regla multiframe, Engulfing.md, 137–138 y 389–393](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/10 PM - Regla multiframe, Engulfing.md:137>) | «ratio mínimo […] 1 a 1; menor a eso no nos interesa». Se mide el camino a la siguiente línea, no un recorrido libre hasta un objetivo lejano. |
| **R2** | [10 PM - Regla multiframe, Engulfing.md, 199–219](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/10 PM - Regla multiframe, Engulfing.md:199>), [12 PM - Multiframe.md, 159–179](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/12 PM - Multiframe.md:159>) y [16 PM.md, 498–499](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/16 PM.md:498>) | SL según patrón/línea y espacio técnico. En 16 se protege por debajo del patrón o de la línea, «lo que quede más lejano». En 12 el porcentaje monetario determina posición, no altera la invalidación técnica. |
| **R3** | [12 PM - Multiframe.md, 125–146](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/12 PM - Multiframe.md:125>) y [17 PM.md, 557–566](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/17 PM.md:557>) | «como buena práctica […] colocar el take profit antes de las zonas». Los checks también pueden frenar el recorrido; no se fija una distancia universal de anticipo. |
| **M1** | [19 PM - Convergencia de frames, RSI, VYK.md, 234–240 y 542–547](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/19 PM - Convergencia de frames, RSI, VYK.md:234>) y [24 PM.md, 159–199](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/24 PM.md:159>) | Indicadores «nunca son gatillos». En 24: «estas tres condiciones de zona gatillo y ratio son […] inmutables»; estructuras y divergencias se registran y comparan como mejoras posteriores. |
| **M2** | [21 PM - Trading Book.md, 119–123, 266 y 723](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/21 PM - Trading Book.md:119>) y [24 PM.md, 77–115](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/24 PM.md:77>) | Autorización para real condicionada a efectividad personal; «70 por ciento o más» y registro continuo. «backtesting […] paper trading […] operativa en real». No es un indicador observable en una vela. |

### Ambigüedades, contradicciones y evolución del criterio

| Tema | Diferencia documentada | Tratamiento en este checklist |
|---|---|---|
| **Qué confirma un impulso** | [1, 407–439](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/1 PM - Velas Movimientos Tendencias.md:407>) y [2, 18–29](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/2 PM - Movimientos, Impulsos.md:18>) parten de estructura; [3, 99–109](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/3 PM - Cajas y Check points.md:99>) exige también check roto; [6, 496–500](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/6 PM - Movimientos, Checks y Pivotes.md:496>) exceptúa el primer impulso. | Evolución didáctica, no tres definiciones intercambiables. No usar sólo un patrón local de máximos/mínimos para generar todos los pivotes del método. |
| **Cruce mínimo por mecha o zona real** | [2, 417–424](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/2 PM - Movimientos, Impulsos.md:417>) acepta superar aunque vuelva; [3, 283–288](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/3 PM - Cajas y Check points.md:283>) aclara que el cruce minúsculo era ejercicio y después se tratarán zonas; [7, 251–276](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/7 PM - Pivotes y Lineas de tendencia.md:251>) distingue permisividad de líneas y checks. | No imponer cierre a toda ruptura, ni convertir un tick en tolerancia final universal. Falta formalizar la franja. |
| **Uso de checks en su propio frame** | [3, 275–280](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/3 PM - Cajas y Check points.md:275>) los presenta como zonas en frame actual y menores; [7, 427–429](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/7 PM - Pivotes y Lineas de tendencia.md:427>) dice que funcionan en menores, no en actuales. | Diferencia no resuelta entre validación estructural, objetivo y entrada. No habilitar automáticamente un check como zona de entrada en su frame sin validación. |
| **Cierre de reversión** | [11, 138 y 355–359](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/11 PM - Gatillos.md:138>) y [16, 439–444](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/16 PM.md:439>) exigen cierre; [23, 403 y 411–427](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/23 PM - Operativa.md:403>) permite anticiparlo y aconseja entrar cerca del cierre. | Perfil inicial propuesto al cierre para reproducibilidad. La modalidad anticipada debe identificarse como tal; una vela abierta no acredita el patrón definitivo. |
| **Punta del movimiento** | [11, 265–281](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/11 PM - Gatillos.md:265>) la exige para reversión; [15, 214–221 y 742–745](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/15 PM - Replay.md:214>) admite rebotes interiores de etapa 2. | Aplicar por etapa y referencia local; no prohibir todas las entradas interiores de canal. |
| **Tendencia y escenarios** | [19, 82–87](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/19 PM - Convergencia de frames, RSI, VYK.md:82>) no invalida toda operación contra estructura; [24, 165–199](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/24 PM.md:165>) propone medir ese filtro después. | No añadir «todos los frames alineados» ni «sólo escenario A» como cuarto requisito. A/B/C no tienen una puntuación matemática estable. |
| **Multiframe básico y excepciones** | [10, 316–321](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/10 PM - Regla multiframe, Engulfing.md:316>) limita frames; [19, 472–478](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/19 PM - Convergencia de frames, RSI, VYK.md:472>) admite bajar más con convergencia continua de divergencias; [24, 425–429](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/24 PM.md:425>) vuelve a separar esa variante avanzada. | Conservar la tabla básica. La excepción no permite elegir libremente cualquier frame inferior. |
| **Confirmación de canal** | [13, 172–192](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/13 PM - Etapa 2.md:172>) desarrolla canal con dos toques por lado; [15, 17–24 y 94–98](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/15 PM - Replay.md:17>) permite anticiparlo con tercer rechazo y camino. | Distinguir canal confirmado y canal anticipado. «Fuerte» o «buen camino» no reciben aquí umbrales inventados. |
| **Exposición simultánea** | [21, 651–658](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/21 PM - Trading Book.md:651>) dice no exceder el riesgo estipulado con varias entradas; [22, 459–474](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/22 PM - Gestion de riesgo.md:459>) distingue correlacionados y no correlacionados. | Documentar la matización; no convertir el ejemplo de riesgo en un porcentaje universal. Es gestión de cuenta, distinta del ratio gráfico. |
| **Márgenes y 50%** | [10, 207–234](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/10 PM - Regla multiframe, Engulfing.md:207>) alterna referencias a vela/patrón; [11, 140–163 y 305–324](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/11 PM - Gatillos.md:140>) distingue mitad del hombro izquierdo y mitad del retesteo. | No mezclar los denominadores. La igualdad y las referencias exactas deben validarse por variante. |
| **Secuencia dentro de una vela** | [1, 306–322](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/1 PM - Velas Movimientos Tendencias.md:306>) reconoce que el camino dibujado «No siempre es así»; [4, 206–238](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/4 PM - Cajas y Rangos.md:206>) y [6, 48–69](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/6 PM - Movimientos, Checks y Pivotes.md:48>) lo utilizan didácticamente. | OHLC no demuestra el orden real entre máximo y mínimo. No dar por conocido un pullback rápido sólo por el color de una vela exterior. |

### Inventario de cobertura y aportes

Todos los archivos siguientes se leyeron completos. El número indica líneas físicas, no cantidad de reglas.

| Archivo | Líneas | Aporte principal al análisis |
|---|---:|---|
| [1 PM - Velas Movimientos Tendencias.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/1 PM - Velas Movimientos Tendencias.md>) | 658 | Velas cerradas, escala logarítmica, movimiento/impulso/tendencia; el camino OHLC es aproximado. |
| [2 PM - Movimientos, Impulsos.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/2 PM - Movimientos, Impulsos.md>) | 456 | Estructura y origen X; superación por mecha; operativa tendencial y contratendencial. |
| [3 PM - Cajas y Check points.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/3 PM - Cajas y Check points.md>) | 463 | Caja, checkpoint, extremo dinámico y dos requisitos del impulso. |
| [4 PM - Cajas y Rangos.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/4 PM - Cajas y Rangos.md>) | 486 | Rangos que aportan información, inside/outside, fallas y refinamiento de rupturas. |
| [6 PM - Movimientos, Checks y Pivotes.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/6 PM - Movimientos, Checks y Pivotes.md>) | 581 | Confirmación de pivotes, excepción del primer impulso e igualdad de extremos. |
| [7 PM - Pivotes y Lineas de tendencia.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/7 PM - Pivotes y Lineas de tendencia.md>) | 515 | Líneas entre pivotes consecutivos, anclajes y vigencia en frame de origen. |
| [8 PM - 1234.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/8 PM - 1234.md>) | 482 | Confirmación y eliminación de líneas, strikes y límites de la regla 1234. |
| [9 PM - Contexto.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/9 PM - Contexto.md>) | 508 | Anchura visual, limpieza contextual y zonas de conflicto. |
| [10 PM - Regla multiframe, Engulfing.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/10 PM - Regla multiframe, Engulfing.md>) | 499 | Tres requisitos forzosos, envolvente, SL, ratio y secuencia multiframe. |
| [11 PM - Gatillos.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/11 PM - Gatillos.md>) | 573 | Siete alternativas, punta, temporalidad y límites de pullbacks. |
| [12 PM - Multiframe.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/12 PM - Multiframe.md>) | 479 | Jerarquía de líneas, mapa multiframe, objetivo antes de zona y stop técnico. |
| [13 PM - Etapa 2.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/13 PM - Etapa 2.md>) | 612 | Figuras temporales, prioridad de etapa 1 y proyecciones con excepciones. |
| [14 PM - Etapa 2.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/14 PM - Etapa 2.md>) | 579 | Figuras neutras/continuación, convergencias y jerarquía contextual. |
| [15 PM - Replay.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/15 PM - Replay.md>) | 867 | A/B/C, canales anticipados, rebotes interiores y registro sin selección retrospectiva. |
| [16 PM.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/16 PM.md>) | 769 | Reitera condiciones inalterables, SL más lejano y relación con siguiente línea. |
| [17 PM.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/17 PM.md>) | 719 | Ejecución, espacio de SL, checks como objetivos y reingresos avanzados. |
| [18 PM - Falla, Trampa, Tradingbook.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/18 PM - Falla, Trampa, Tradingbook.md>) | 756 | Variantes avanzadas de fallas/trampas y registro de objetivos 1/1,5/2R. |
| [19 PM - Convergencia de frames, RSI, VYK.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/19 PM - Convergencia de frames, RSI, VYK.md>) | 771 | Indicadores como filtros posteriores; excepción avanzada multiframe. |
| [20 PM - Bolsas, Estructuras en divergencias.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/20 PM - Bolsas, Estructuras en divergencias.md>) | 406 | Requisitos de sesiones por mercado y divergencias que cambian antes del cierre. |
| [21 PM - Trading Book.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/21 PM - Trading Book.md>) | 791 | Selección personal para real y distinción entre práctica exhaustiva y operativa. |
| [22 PM - Gestion de riesgo.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/22 PM - Gestion de riesgo.md>) | 869 | Capital operativo, riesgo por bloques, drawdown y correlación de exposiciones. |
| [23 PM - Operativa.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/23 PM - Operativa.md>) | 662 | Preparación y ejecución; anticipación al cierre, pullbacks y deslizamiento. |
| [24 PM.md](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/24 PM.md>) | 733 | Tres condiciones inmutables, evolución del grimorio y separación base/avanzado/holding. |

**Otros requisitos documentados:** sesiones de forex/acciones en [20, 30–35, 55–93](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/20 PM - Bolsas, Estructuras en divergencias.md:30>); liquidez/legibilidad y cierre intradía en [24, 681–733](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/24 PM.md:681>); límite de pérdida y capital operativo en [22, 177–197 y 526–540](<C:/Users/jose_/Documents/ChatGPT/Trading/transcripciones/22 PM - Gestion de riesgo.md:177>). Son restricciones operativas adicionales, no motivo para añadir medias móviles, osciladores o filtros de volumen a las tres condiciones gráficas.

## E. Propuesta del futuro indicador

### Alcance mínimo útil

Un **panel de evaluación de un setup**, inicialmente con zonas y referencias manuales, resulta más fiel que prometer reconocimiento automático completo del mapa. El panel tendrá tres filas —Zona, Gatillo y Ratio— y una razón verificable para cada estado.

| Estado | Significado |
|---|---|
| **Cumple** | Los datos y criterios definidos permiten comprobar la condición. |
| **No cumple** | Se conoce un requisito incumplido; mostrar cuál y su valor. |
| **Pendiente** | Falta una definición, una referencia, el cierre requerido o evidencia suficiente de secuencia. |

**Validación general:** si alguna fila no cumple, «Setup no válido»; si ninguna falla pero existe algún pendiente, «Evaluación pendiente»; sólo si todas cumplen, «Setup gráfico válido». La selección de colores no debe ser el único medio de comunicar esos estados. La habilitación para operar en real se muestra aparte o se deja expresamente fuera del indicador; no se presume por el color del panel.

**Elementos en el gráfico:** franja de zona y frame de origen; etiqueta de etapa; extremo de referencia; fases o velas del gatillo; entrada prevista, SL, primer obstáculo y objetivo; distancias de riesgo/beneficio y ratio. Diferenciar el momento en que ocurre un extremo del momento en que queda confirmado.

**Configuración necesaria:** frames válidos del modelo; dirección y familia; límites/anclajes de zona; precios de E/S/T o referencias técnicas; perfil temporal; y sólo las tolerancias que se hayan validado. Para el modo manual, la vigencia y el primer obstáculo también deben declararse. No se añaden RSI, VYK, medias, ATR ni un filtro de volumen: su presencia en partes avanzadas no justifica convertirlos en requisitos de esta versión.

### Viabilidad realista en Pine Script v6

La evaluación técnica siguiente se contrastó con documentación oficial de TradingView el 22 de octubre de 2026. Las fuentes externas verifican capacidades del lenguaje; **no aportan reglas a la estrategia**.

| Necesidad | Viabilidad y restricción |
|---|---|
| Comparar OHLC, extremos y E/S/T | Es viable calcular desigualdades y el ratio con datos disponibles. La selección correcta del mapa sigue siendo el principal trabajo pendiente. |
| Parámetros y zonas manuales | Se pueden introducir precios, tiempos y temporalidades; los inputs de precio/tiempo admiten selección interactiva. Una línea dibujada con la barra de herramientas no es legible directamente por Pine: hay que aportar anclajes al script o dibujarla desde él. [Inputs](https://www.tradingview.com/pine-script-docs/concepts/inputs/), [Lines and boxes](https://www.tradingview.com/pine-script-docs/visuals/lines-and-boxes/). |
| Detectar pivotes | Existen pivotes estándar, pero se confirman varias velas después. No son equivalentes por sí solos al pivote estructural del curso. Usarlos requeriría validar esa sustitución y registrar el retraso; dibujar el punto en una vela pasada no significa haberlo conocido entonces. [Visuals: pivots](https://www.tradingview.com/pine-script-docs/faq/visuals/). |
| Confirmar reversión al cierre | `barstate.isconfirmed` permite exigir cierre en el contexto del gráfico. No resuelve por sí solo una consulta a otro frame: no debe usarse dentro de `request.security()` como garantía de confirmación. [Bar states](https://www.tradingview.com/pine-script-docs/concepts/bar-states/). |
| Obtener contexto de frames mayores | Es viable solicitar datos con `request.security()`. Para un perfil sin repaint de datos superiores, utilizar valores de una vela superior ya cerrada y el patrón documentado de desplazamiento histórico junto con `lookahead_on`; este último sin desplazar introduce anticipación indebida. La información llega con la latencia de confirmación correspondiente. [Other timeframes and data](https://www.tradingview.com/pine-script-docs/concepts/other-timeframes-and-data/#avoiding-repainting). |
| Reconocer un pullback intravela | OHLC de la vela principal no prueba las tres fases en orden. Los datos de frames inferiores pueden ayudar; `request.security_lower_tf()` entrega intrabarras, pero su disponibilidad y cobertura histórica son limitadas. Un frame inferior no demuestra todo el recorrido de ticks dentro de sus propias velas. Mantener Pendiente cuando no se pueda establecer la secuencia. [Other data and timeframes](https://www.tradingview.com/pine-script-docs/faq/other-data-and-timeframes/), [Other timeframes and data](https://www.tradingview.com/pine-script-docs/concepts/other-timeframes-and-data/). |
| Panel y marcas | Tablas para las tres condiciones; líneas y cajas para niveles/franjas; texto para causas y fechas de confirmación. Las tablas muestran su último estado, por lo que no bastan como registro de estados históricos. [Tables](https://www.tradingview.com/pine-script-docs/visuals/tables/). |

### Prevención de repainting y sesgo retrospectivo

Estas son **propuestas de implementación pendientes de validación**, coherentes con la necesidad de reproducibilidad:

1. Una reversión abierta puede mostrarse como candidata, pero no como confirmada en el perfil de cierre. La modalidad anticipada debe distinguirse visualmente y no confundirse con una confirmación histórica.
2. No utilizar un pivote, una línea o una reclasificación antes de su fecha de confirmación. Las sesiones permiten corregir el mapa con contexto nuevo; conservar qué mapa y qué datos existían al evaluar cada setup.
3. Separar actualizar o eliminar líneas para el análisis actual de borrar evaluaciones anteriores. Que el contexto cambie después no demuestra que la señal previa nunca existió.
4. No reconstruir el orden intravela por el color de la vela. Si no hay evidencia ordenada, no certificar el pullback rápido ni el instante exacto de entrada.
5. Registrar cualquier cambio manual de anclajes y su fecha de vigencia; un indicador recalculado con zonas elegidas hoy no equivale a una prueba histórica causal.

TradingView documenta que datos en desarrollo y dibujos retrospectivos pueden alterar lo observado entre histórico y tiempo real; confirmar datos reduce esas diferencias a cambio de retraso. No se promete ausencia total de cambios por revisiones del proveedor. [Repainting](https://www.tradingview.com/pine-script-docs/concepts/repainting/).

### Límites conocidos

- Las transcripciones remiten repetidamente a gráficos y gestos —«aquí», «esta mitad», «esta línea»— que no están incluidos en el corpus. Algunas geometrías no pueden reconstruirse con certeza sólo desde el texto.
- Zona, rango similar, indecisión, cercanía, rechazo fuerte y ciertos márgenes no tienen una cuantificación final consistente. Se pueden mantener manuales; automatizarlos sin decidirlos cambiaría la estrategia.
- Las figuras de etapa 2, fallas/trampas y convergencias avanzadas pueden quedar fuera del primer detector automático. Su reconocimiento no es necesario para ofrecer el panel básico de tres condiciones con referencias manuales.
- El indicador no puede inferir del gráfico la estadística personal del Trading Book, capital real, riesgo agregado o autorización para operar en real. Horarios, disponibilidad de datos y costes requieren configuración o verificación aparte.
- No se han comprobado señales sobre un activo, medido rentabilidad ni ejecutado backtesting. Este documento tampoco convierte las afirmaciones de rendimiento de las clases en resultados verificados.

## F. Preguntas pendientes

Sólo se enumeran decisiones que faltan para convertir lo ambiguo en criterios programables:

1. **Zonas:** ¿se proporcionarán manualmente límites y anclajes o se exige reconstruir el mapa completo? En este último caso, ¿cómo se delimitan influencia, cruces y strikes, se agrupan zonas y se habilitan los checks como entradas en su propio frame?
2. **Estructura causal:** ¿cómo se decide un movimiento/rango ambiguo, especialmente una vela exterior, sin inventar su orden intravela, y desde qué momento queda confirmado cada pivote o cambio de mapa?
3. **Etapas y variantes:** ¿el primer alcance reconocerá sólo etapa 1 o también figuras de etapa 2, y cuáles de los siete gatillos se automatizarán primero? Las variantes restantes pueden mantenerse manuales; no deben declararse inválidas por no estar implementadas.
4. **Medidas de gatillo:** ¿qué tolerancias definen similitud de pinzas, indecisión de stars, contacto del pullback perfecto y cercanía del corto? ¿Se acepta la igualdad exacta al 50%, y qué límites se usan para la penetración de las reversiones? Validar las traducciones de punto medio y distancia de C2, especialmente para líneas inclinadas.
5. **Tiempo de entrada:** ¿reversión sólo al cierre o también anticipada? Para pullbacks, ¿se conservará activación intravela con evidencia inferior/manual, o se adoptará expresamente una variante posterior al cierre?
6. **Stop, entrada y objetivo:** ¿qué margen técnico se usará por activo/frame, cómo se decide el primer obstáculo y cuánto se anticipa el TP, si se anticipa? ¿Cuál será el precio de entrada de referencia y se evaluará ratio bruto o ajustado por costes?
7. **Geometría:** ¿cómo se representarán las líneas y proyecciones en precio/tiempo para reproducir la escala logarítmica del material, sin depender de ángulos o distancias de pantalla?

