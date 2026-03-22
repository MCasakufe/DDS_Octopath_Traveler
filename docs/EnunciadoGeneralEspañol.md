# Proyecto: Octopath Traveler

**Pontificia Universidad Católica de Chile**  
**Escuela de Ingeniería**  
**Departamento de Ciencia de la Computación**  
**IIC2113 Diseño Detallado de Software**

**Autores del enunciado:** Javiera Ignacia Pinto Santa María, Matías Andrés Poblete Farías

---

## 1. Introducción

*Octopath Traveler* es una serie de videojuegos del género JRPG desarrollados por Square Enix junto a Acquire.

Su historia comienza en 2018, cuando se lanza el primer juego de la franquicia, titulado *Octopath Traveler*. Este fue publicado primero para la consola Nintendo Switch y luego para Microsoft Windows, obteniendo críticas mayormente positivas, en las cuales se destaca principalmente el estilo artístico del juego, una mezcla entre diseños 2D y fondos en 3D. Desde entonces se han publicado dos juegos más, *Octopath Traveler II* en 2023 y *Octopath Traveler 0* en 2025, además de una versión móvil de descarga gratuita, *Octopath Traveler: Champions of the Continent*.

La saga destaca por su sistema de combate por turnos y su estructura narrativa única, donde cada personaje cuenta con su propia historia, permitiendo al jugador explorar el mundo desde múltiples perspectivas. Su distintivo estilo visual, conocido como “HD-2D”, combina píxeles clásicos con efectos modernos de iluminación y profundidad, consolidando a *Octopath Traveler* como una de las franquicias más reconocibles y aclamadas del género JRPG contemporáneo.

**Figura 1.** Personajes principales de *Octopath Traveler*.

El objetivo de este proyecto es implementar una versión simplificada y modificada del sistema de combate por turnos de *Octopath Traveler*. A grandes rasgos, el juego consiste en un enfrentamiento entre tu equipo y bestias enemigas, donde se busca atacar sus debilidades para ganar turnos.

---

## 2. Setup

En el juego, el equipo del jugador, formado por viajeros, se enfrentará en combate a un equipo enemigo, formado por bestias. Cada uno de los equipos se formará de la siguiente manera.

### Equipo del jugador

- Debe conformarse de 1 a 4 viajeros.
- No pueden haber viajeros repetidos.
- Cada viajero puede tener un máximo de 8 habilidades activas, pero también podría no tener habilidades asignadas.
- Cada viajero tiene un máximo de 4 habilidades pasivas, pero también podría no tener ninguna habilidad asignada.
- Un viajero no puede tener habilidades repetidas. Por ejemplo, si Cyrus tiene la habilidad `Fireball`, no puede tener un segundo `Fireball` en el listado de habilidades.

### Equipo enemigo

- Debe estar conformado de 1 a 5 bestias.
- Las bestias tienen una única habilidad.
- No pueden haber bestias repetidas en el equipo enemigo.

Luego de armar los equipos, las unidades se posicionarán sobre un tablero en donde se desarrollará el combate.

- El jugador cuenta con 4 posiciones, donde se ubicarán sus viajeros, utilizando los espacios de izquierda a derecha.
- El enemigo cuenta con 5 posiciones en el tablero, en donde se ubicarán las bestias, llenando las posiciones de izquierda a derecha.

**Figura 2.** Batalla entre un equipo de 4 viajeros contra 5 bestias. El equipo de las bestias aparece en la parte superior del tablero y los viajeros en la inferior, junto con la ronda actual y la siguiente, de izquierda a derecha, en la parte superior de la figura.

Si el jugador posee menos de 4 viajeros o el enemigo posee menos de 5 bestias, se considerará que ciertos espacios del tablero quedan vacíos.

---

## 3. Unidades

Antes de presentar el flujo del juego, se ahondará en los distintos atributos que puede poseer una unidad. Si bien tanto viajeros como bestias poseen atributos en común, existen diferencias que serán fundamentales al momento de determinar el flujo del juego.

### Viajeros

Los viajeros se caracterizan por tener un conjunto de habilidades activas y habilidades de apoyo, las cuales el jugador podrá utilizar durante el combate. El viajero posee los siguientes atributos:

- **Nombre:** es el nombre de la unidad y su identificador único.
- **Stats:** son una serie de números que determinarán qué tan buena es la unidad en distintos roles.
- **Armas:** una lista con las distintas armas que puede tener un viajero para realizar un ataque básico.
- **Habilidades activas:** son habilidades que los viajeros pueden usar para atacar o provocar efectos especiales en el juego.
- **Habilidades pasivas:** son habilidades que los viajeros poseen de forma pasiva, provocando efectos sobre sí mismo o sobre otras unidades.

### Bestias

Por otro lado, las bestias poseen una única habilidad que usarán en combate, además de tener debilidades y *Shields*.

- **Nombre:** es el nombre de la unidad y su identificador único.
- **Stats:** son una serie de números que determinarán qué tan buena es la unidad en distintos roles.
- **Habilidad:** es la habilidad que la bestia puede usar para atacar o provocar efectos especiales en el juego.
- **Debilidades:** determina si la bestia será débil contra algún tipo de ataque, recibiendo daño extra.
- **Shields:** es un número que indica la cantidad de veces que una bestia puede recibir un ataque al que es débil. Al llegar a 0, la bestia entra en un estado de debilidad llamado **Breaking Point**.

### 3.1. Stats

Los *stats* son parte importante del flujo del juego, determinando el resultado de ciertas acciones. Estos son personales para cada unidad y presentan diferencias entre viajeros y bestias.

#### Stats de viajeros

- **HP máximo:** vida máxima que puede tener la unidad. También representa la vida con la que la unidad inicia el combate.
- **HP actual:** vida actual de la unidad. Si este valor llega a 0, la unidad muere. Este valor se encuentra entre 0 y HP máximo. Por simplicidad a este *stat* se le suele llamar simplemente **HP**.
- **SP máximo:** determina la cantidad máxima de *Skill Points* que puede tener una unidad.
- **SP actual:** determina la cantidad actual de *Skill Points* que posee la unidad. Estos pueden ser gastados para que la unidad utilice una de sus habilidades activas. Este valor se encuentra entre 0 y SP máximo. Por simplicidad a este *stat* se le suele llamar simplemente **SP**.
- **Phys Atk:** determina la potencia de los ataques físicos.
- **Phys Def:** determina qué tan resistente es la unidad a golpes físicos del rival.
- **Elem Atk:** determina la potencia de los ataques elementales.
- **Elem Def:** determina qué tan resistente es la unidad a golpes elementales del rival.
- **Speed:** determina la prioridad durante los turnos.

#### Stats de bestias

Las bestias se diferencian en que no poseen valores asociados a SP:

- **HP máximo:** vida máxima que puede tener la unidad. También representa la vida con la que la unidad inicia el combate.
- **HP actual:** vida actual de la unidad. Si este valor llega a 0, la unidad muere. Este valor se encuentra entre 0 y HP máximo. Por simplicidad a este *stat* se le suele llamar **HP**.
- **Phys Atk:** determina la potencia de los ataques físicos.
- **Phys Def:** determina qué tan resistente es la unidad a golpes físicos del rival.
- **Elem Atk:** determina la potencia de los ataques elementales.
- **Elem Def:** determina qué tan resistente es la unidad a golpes elementales del rival.
- **Speed:** determina la prioridad durante los turnos.

---

## 4. Sistemas de juego

Una vez conformados los equipos, el jugador deberá tomar decisiones sobre sus viajeros para intentar vencer al equipo enemigo de bestias. Por su parte, el equipo de bestias será controlado de forma automática.

Antes de ahondar en las mecánicas del combate, el jugador debe comprender elementos esenciales del juego: los ataques, el sistema de turnos, las acciones que puede tomar cada unidad, el boosting y el breaking point.

### 4.1. Turnos

*Octopath Traveler* es un juego por turnos, donde el orden en que las unidades atacan puede variar dependiendo de qué decisiones se tomen. La idea es lograr maximizar los turnos del jugador y minimizar los turnos del enemigo, para así realizar más ataques y disminuir el daño recibido.

El juego se desarrolla en **rondas**. En cada ronda, todas las unidades, viajeros y bestias, tienen la oportunidad de actuar una vez. Cada una de esas acciones corresponde a un turno. Ciertas habilidades podrían otorgarle al viajero la oportunidad de atacar de nuevo, teniendo más de un turno por ronda. Cuando todas las unidades han jugado su turno, la ronda termina y comienza la siguiente.

De base, el orden de los turnos dentro de cada ronda se decide por la velocidad de las unidades en combate, tanto viajeros como bestias, con las cuales se arma una cola de turnos en donde las unidades con mayor **Speed** jugarán primero. Sin embargo, el uso de ciertas habilidades, la explotación de las debilidades y el **Breaking Point** pueden alterar el orden de la cola de turnos.

### 4.2. Tipos de ataque y debilidades

En el juego existen distintos tipos de ataques, los cuales pueden resultar útiles en diversas situaciones. Los tipos de ataque se pueden clasificar en dos categorías.

#### Ataques físicos

Aquellos que determinan su daño en base a los *stats* **Phys Atk** y **Phys Def**.

Tipos físicos:
- `Sword`
- `Spear`
- `Axe`
- `Dagger`
- `Bow`
- `Stave`

#### Ataques elementales

Aquellos que determinan su daño en base a los *stats* **Elem Atk** y **Elem Def**.

Tipos elementales:
- `Fire`
- `Ice`
- `Lightning`
- `Wind`
- `Light`
- `Dark`

Cada bestia del equipo enemigo tiene al menos una debilidad a algún tipo de ataque, ya sea físico o elemental. Atacar a una bestia con su debilidad hará que esta reciba una mayor cantidad de daño y puede cambiar el flujo del combate.

En casos muy especiales pueden existir ataques físicos o elementales que no tengan tipo, así como ataques que no sean ni físicos ni elementales.

### 4.3. Acciones

Las unidades podrán realizar diversas acciones cuando sea su turno de actuar, las que pueden tener diversos efectos en el juego.

En este punto hay una diferencia entre viajeros y bestias. Los viajeros pueden escoger entre distintas acciones, a diferencia de las bestias que actúan de forma automática.

#### Viajeros

Cada vez que llega el turno de un viajero, el jugador deberá seleccionar entre las siguientes opciones:

- **Ataque básico:** selecciona una de sus armas y la utiliza para atacar a un enemigo. Todas las armas son del tipo físico, por lo tanto, la acción de ataque básico siempre significa un ataque físico.
- **Usar habilidad activa:** selecciona una de las habilidades activas que posee el viajero. Las habilidades pueden ser de daño físico o elemental, o bien tener efectos sobre el combate.
- **Defender:** el viajero gasta su turno para aumentar su resistencia. Por el resto de la ronda, recibirá 50 % del daño que recibiría normalmente y obtendrá prioridad de turno en la siguiente ronda.
- **Huir:** huye del combate, acabando el juego y dando por ganadores a los enemigos.

#### Bestias

Cada vez que llega el turno de una bestia esta solo tendrá la opción de utilizar su habilidad. Las habilidades pueden ser de daño físico o elemental, o bien tener efectos sobre el combate.

Dado que las bestias solo tienen la acción de utilizar su habilidad, en ningún momento se da una elección sobre la acción que realizará una bestia. El efecto de la habilidad, así como a quién afectará, se encuentra descrito en la definición de la habilidad.

### 4.4. Boosting

Cada uno de los viajeros tendrá una cantidad de **Boost Points**, llamados **BP**.

En el turno de cada viajero, el jugador puede escoger gastar BP para realizar su acción con *boosting*, amplificando su efecto. El *boosting* tiene el siguiente efecto en cada acción del viajero:

- **Ataque básico:** se realiza un ataque adicional por cada BP gastado. Por ejemplo, si un viajero realiza ataque básico con 2 BP, entonces realizará su ataque normal y 2 ataques adicionales.
- **Usar habilidad activa:** potencia el efecto de la habilidad que se utiliza. El aumento de potencia depende de cada habilidad y su efecto. Por ejemplo, habilidades activas que infligen daño pasan a infligir una mayor cantidad de daño por cada BP gastado.

Las acciones **Defender** y **Huir** no permiten utilizar BP.

Cada viajero iniciará el combate con **1 BP** y, al final de cada ronda, todos los viajeros vivos obtendrán **1 BP adicional**, pudiendo llegar a un máximo de **5 BP**. Al momento de realizar una acción con *boosting*, el viajero puede gastar hasta un máximo de **3 BP**, independiente de si tiene más. Además, si un viajero realizó una acción con *boosting* en una ronda, no recibirá 1 BP al final de esta.

### 4.5. Shields

Cada bestia tiene una cantidad de **Shields** asociados. Este es un número que indica cuántas veces hay que atacar alguna de sus debilidades para que la bestia entre al estado **Breaking Point**.

Como se puede apreciar en la Figura 2, la bestia *Chubby Cait* posee 4 Shields. Con esto, es necesario golpearla 4 veces con alguna de sus debilidades para que sus Shields lleguen a 0 y entre al estado **Breaking Point**. Si un ataque hace 0 de daño, entonces la bestia no perderá Shields por el ataque.

Una vez los Shields de la bestia llegan a cero, esta entrará en un **Breaking Point** durante la ronda actual y la siguiente. En este estado:

- la bestia no es capaz de realizar ningún tipo de acción;
- además recibe daño adicional de cualquier ataque que reciba.

Al pasar las dos rondas, la bestia sale del estado de Breaking Point, reinicia su cantidad de Shields y obtiene prioridad en el orden de turnos de la siguiente ronda.

### 4.6. Manejo de la cola

Dentro de una ronda, todas las unidades, tanto viajeros como bestias, se deben ordenar bajo ciertas reglas para así saber cuándo es su turno de atacar.

Para generar la cola de turnos, se seguirá el siguiente algoritmo:

1. Se posicionan primero en la cola las bestias que se recuperen del estado de **Breaking Point**. En caso de que dos o más bestias se recuperen en la misma ronda, estas se ordenarán por su velocidad. Si se mantiene el empate, se ordenarán por orden de tablero, desde la más a la izquierda hasta la más a la derecha.
2. Después, se posicionarán los viajeros que hayan utilizado la acción **Defender** en la ronda anterior. Si dos o más viajeros se encuentran en esta situación, se ordenarán primero por velocidad y luego por orden de tablero.
3. Luego, se posicionarán las unidades que, por efecto de alguna habilidad, aumentaron su prioridad en la ronda. Si dos o más unidades se encuentran en esta situación, se prioriza a viajeros por sobre bestias. Si el empate se mantiene, se ordena por velocidad y luego por orden de tablero.
4. Las unidades que no entren en ninguna de las categorías anteriores se ordenarán por su velocidad. Si dos o más unidades tienen el mismo valor en **Speed**, se dará prioridad a los viajeros por sobre las bestias. Si dos o más bestias, o dos o más viajeros, tienen el mismo valor de Speed, se ordenarán por orden de tablero.
5. Finalmente, se posicionarán las unidades que, por efecto de alguna habilidad, disminuyeron su prioridad en la ronda. Si dos o más unidades se encuentran en esta situación, se priorizan los viajeros sobre las bestias. Si el empate se mantiene, se ordenarán por velocidad y luego por orden de tablero.
6. Si una unidad está presente en dos o más categorías, manda la más prioritaria. Por ejemplo, si una bestia se recupera del estado de Breaking Point y además está bajo el efecto de alguna habilidad que le baja su prioridad, entonces manda la primera categoría.

#### Ejemplo de orden por velocidad

Supongamos una batalla de 4 viajeros contra 5 bestias, con los siguientes *stats* de velocidad:

- Ophilia, Speed: 291
- Primrose, Speed: 392
- Therion, Speed: 445
- Olberic, Speed: 253
- Chubby Cait, Speed: 500
- Heavenwing, Speed: 275
- Monarch, Speed: 152
- Demon Goat, Speed: 267
- Dreadwolf, Speed: 325

Siguiendo únicamente la velocidad, el orden sería:

1. Chubby Cait
2. Therion
3. Primrose
4. Dreadwolf
5. Ophilia
6. Heavenwing
7. Demon Goat
8. Olberic
9. Monarch

**Figura 7.** Tablero al inicio de la ronda, ordenando a las unidades en base a su velocidad.

Ahora supongamos los siguientes escenarios:

- Ophilia posee y se le activa una habilidad pasiva que otorga prioridad en la cola de turnos.
- En la ronda anterior, Olberic utilizó **Defender**.
- En la ronda anterior, Primrose utilizó una habilidad sobre Dreadwolf que tiene por efecto despriorizar a la unidad por dos rondas.
- En la ronda anterior, Therion utilizó una habilidad que, además de hacer daño, le otorga prioridad en la cola de turnos.
- Durante la ronda anterior, Dreadwolf entró en **Breaking Point**.

Entonces, la ronda quedaría ordenada considerando todos esos efectos.

**Figura 8.** Tablero al inicio de la ronda, ordenado tomando en consideración todos los escenarios.

En la figura se observan varios casos:

1. Olberic aparece primero en la cola de la ronda actual, porque utilizó **Defender** en la ronda anterior.
2. Le siguen Therion y Ophilia, ambos bajo efectos que priorizan su turno, por lo que se ordenan entre ellos en base a su velocidad.
3. Luego aparece el resto de unidades no afectadas por una categoría especial, ordenadas por velocidad.
4. Dreadwolf no aparece en la ronda actual porque está en **Breaking Point** desde la ronda anterior. Sí aparece primero en la cola de la ronda siguiente, porque recuperarse de Breaking Point tiene prioridad sobre una despriorización aplicada por habilidad.

---

## 5. Flujo de juego

Ahora que conocemos los elementos y los sistemas del juego, se profundiza en el flujo general del programa.

En situaciones normales, el flujo del juego será el siguiente:

1. Comienza una ronda y se ordena la cola de turnos actual y siguiente con las reglas mencionadas anteriormente.
2. Siguiendo el orden definido, cada unidad actuará en su turno realizando alguna de las acciones disponibles.
3. Si en cualquier momento de la ronda una bestia entra en **Breaking Point**, esta pierde su turno en la ronda actual, en caso de no haber actuado aún, y en la ronda siguiente.
4. Si en cualquier momento de la ronda una unidad muere, esta permanecerá muerta en el tablero por el resto del juego, en la misma posición y no podrá actuar.
5. Si en cualquier momento de la ronda una unidad actúa de tal forma que altera la cola de turnos de la siguiente ronda, se debe editar la cola de turnos de la siguiente ronda para que refleje el cambio.
6. Si en cualquier momento mueren todos los viajeros, el juego finaliza y el jugador pierde.
7. Si en cualquier momento mueren todas las bestias, el juego finaliza y el jugador gana.
8. En el momento en que ya no quedan turnos por jugar, se considera la ronda finalizada.
9. Una vez finalizada una ronda, cada viajero vivo que no haya usado *boosting* obtendrá 1 BP y, a continuación, se inicia una nueva ronda, la cual sucederá de la misma forma y bajo las mismas reglas.

### 5.1. Orden de turnos

Como se mencionó anteriormente, el orden en que las unidades podrán actuar dependerá de cómo se comparen sus *stats*. A continuación se ahonda en cómo distintas interacciones pueden hacer que este orden cambie.

Al inicio de la partida, el orden de los turnos se define según la *stat* **Speed** de forma descendiente. Si hay más de una unidad con el mismo valor, se priorizará a los viajeros por sobre las bestias y a las unidades que se encuentren más a la izquierda en el tablero.

#### Ejemplo base

Supongamos una batalla de 4 viajeros contra 2 enemigos, donde se tienen las siguientes *stats* de velocidad:

- Ophilia, Speed: 291
- Tressa, Speed: 240
- Therion, Speed: 445
- H’aanit, Speed: 350
- Mutant Mushroom, Speed: 131
- Demon Deer, Speed: 100

El orden inicial sería:

1. Therion
2. H’aanit
3. Ophilia
4. Tressa
5. Mutant Mushroom
6. Demon Deer

**Figura 9.** Tablero al inicio del turno, con el orden de la cola de la ronda actual y la siguiente.

Luego de que una unidad juegue su turno, el orden avanza. Es decir, la unidad que acaba de actuar sale de la cola, permitiendo que se lleve a cabo el turno del resto de personajes. Una vez la cola de turnos esté vacía, se da por finalizada la ronda y comienza una nueva.

Tomando el ejemplo anterior, si Therion realiza una acción, entonces el orden cambiará a:

1. H’aanit
2. Ophilia
3. Tressa
4. Mutant Mushroom
5. Demon Deer

**Figura 10.** Tablero luego de que Therion realizara una acción.

#### Ejemplo con Breaking Point

Supongamos ahora que H’aanit en su turno ataca a Mutant Mushroom y baja su contador de Shields a 0, provocándole un **Breaking Point**. Una bestia en estado de Breaking Point pierde su turno de la ronda actual, si no ha actuado aún, y de la ronda siguiente.

**Figura 11.** Tablero luego de que H’aanit llevase a Mutant Mushroom a estado de Breaking Point.

Pasadas dos rondas, la actual y la siguiente, Mutant Mushroom sale del estado de Breaking Point, tiene prioridad en el orden de turnos y recupera sus Shields. El resto de las unidades se ordena bajo las reglas anteriormente mencionadas.

**Figura 12.** Tablero luego de que Mutant Mushroom saliese del estado de Breaking Point.

Como se puede observar en la figura 12, los viajeros poseen 3 BP, esto porque pasaron 2 rondas desde el inicio del juego. Al final de cada ronda, se suma 1 BP a todos los viajeros, a menos que los utilicen.

Supongamos que Therion en su turno utiliza 2 BP contra Mutant Mushroom, llevándolo de nuevo al estado de Breaking Point.

**Figura 13.** Tablero luego de que Therion llevase a Mutant Mushroom a estado de Breaking Point utilizando 2 BP.

Una vez terminada esta ronda, se sumará a todos 1 BP menos a Therion.

**Figura 14.** Tablero al inicio de la siguiente ronda, donde todos sumaron 1 BP menos Therion.

Si en esta ronda Tressa usa la acción **Defender**, además de otorgarle una reducción del 50 % del daño hasta el final de la ronda, tendrá prioridad de turno en la siguiente ronda. Al mismo tiempo, Mutant Mushroom saldrá del estado de Breaking Point. Como se mencionó en las reglas anteriores, tiene prioridad una bestia saliendo del estado de Breaking Point y luego un viajero que defiende.

**Figura 15.** Tablero al inicio de la siguiente ronda, con Mutant Mushroom de primera prioridad, luego Tressa y después el resto, siguiendo las reglas.

---

## 6. Combate

Ahora se ahonda en detalle en los aspectos más específicos del combate, desde el cálculo del daño hasta el manejo de decimales.

### 6.1. Cálculo del daño

El daño que realizará una unidad será calculado en base a sus *stats* y al tipo de ataque que realice. Este daño es un número que se le resta al HP de la unidad atacada.

La fórmula base es:

```text
Daño = [stat ofensiva] × [Modificador] − [stat defensiva]
```

La variable **[Modificador]** tomará el valor que corresponda según la potencia del ataque. Los ataques básicos tienen un modificador de **1.3**, mientras que cada habilidad tendrá su propio modificador.

#### Ejemplo

Se tiene un combate donde la viajera Tressa atacará a la bestia enemiga Meep.

**Tressa**
- Max HP: 275
- Max SP: 50
- Phys Atk: 88
- Phys Def: 80
- Elem Atk: 88
- Elem Def: 80
- Speed: 72

**Meep**
- Max HP: 212
- Phys Atk: 106
- Phys Def: 20
- Elem Atk: 106
- Elem Def: 17
- Speed: 75

Si Tressa ataca a Meep con un ataque básico de tipo `Spear`, el cálculo del daño sería:

```text
Daño = [stat ofensiva] × [Modificador] − [stat defensiva]
     = [Phys Atk] × [Modificador] − [Phys Def]
     = 88 × 1.3 − 20
     = 94.4
     ≈ 94
```

Con esto, Tressa ejecutaría **94** de daño a Meep, dejándolo con:

```text
212 − 94 = 118 HP
```

Un ataque físico utiliza **Phys Atk** del atacante y **Phys Def** del enemigo, mientras que un ataque elemental utiliza **Elem Atk** del atacante y **Elem Def** del enemigo.

### 6.2. Daño con debilidades

Al cálculo de daño anterior se le debe añadir un multiplicador en caso de que el objetivo presente debilidad al tipo de ataque recibido o se encuentre en estado de **Breaking Point**.

- Ser débil a un tipo de ataque significa recibir **50 % de daño adicional**.
- Estar en **Breaking Point** también representa **50 % de daño adicional** al recibir ataques de cualquier tipo.
- Ambos modificadores son acumulables.

La comparación queda así:

| Situación | Estado normal | Breaking Point |
|---|---|---|
| Ataque normal | Daño final = Daño base | Daño final = Daño base × 1.5 |
| Ataque con debilidad | Daño final = Daño base × 1.5 | Daño final = Daño base × 2 |

**Figura 17.** Comparación de daños con debilidades y Breaking Point.

#### Ejemplo con debilidad

Supongamos ahora que Tressa ataca a Meep con un ataque básico de tipo `Bow`, tipo al que Meep es débil. El cálculo sería:

```text
Daño = ([stat ofensiva] × [Modificador] − [stat defensiva]) × [Multiplicador]
     = ([Phys Atk] × [Modificador] − [Phys Def]) × [Multiplicador]
     = (88 × 1.3 − 20) × 1.5
     = 94.4 × 1.5
     = 141.6
     ≈ 141
```

Con esto, Tressa ejecutaría **141** de daño a Meep, dejándolo con:

```text
212 − 141 = 71 HP
```

El daño realizado siempre debe ser mayor o igual a 0. Por lo tanto, en cualquier caso donde se dé un cálculo menor a 0, la unidad realizará **0 de daño**.

### 6.3. Manejo de decimales

Para facilitar el testeo automático del juego, se evitará usar números decimales al momento de realizar cálculos. En particular, los distintos cálculos del juego producen resultados decimales, los cuales deben ser **truncados al entero más cercano por debajo**.

El valor que será truncado será el resultado final del cálculo del daño, luego de haber aplicado cualquier tipo de modificador de daño.

Ejemplos:

```text
Daño = ⌊X × M − Y⌋
```

donde:
- `X` es la stat ofensiva,
- `M` es el modificador,
- `Y` es la stat defensiva.

Si existe debilidad o ruptura:

```text
Daño = ⌊(X × M − Y) × R⌋
```

donde `R` representa el modificador de ruptura.

En resumen, se truncará el valor final luego de aplicar todos los modificadores correspondientes.

---

## 7. Habilidades

En *Octopath Traveler* existen dos tipos de habilidades: **habilidades activas** y **habilidades pasivas**. Las primeras son una acción que se ejecuta durante el turno del viajero, mientras que las segundas se activarán automáticamente sin intervención del jugador siempre que se cumplan ciertas condiciones. Adicionalmente, están las habilidades que utilizarán las bestias, que funcionan de manera similar a las habilidades activas.

Independiente del tipo de habilidad, estas se pueden describir como un conjunto de efectos con un objetivo.

### 7.1. Objetivos

Existen distintos tipos de objetivos que varían según la habilidad. Para este proyecto se consideran 6 formas de seleccionar objetivos para una habilidad:

- **Single:** selecciona un único personaje entre los enemigos. Para el caso de las bestias, selecciona un único personaje entre los viajeros.
- **Enemies:** afecta a todos los personajes enemigos. Para el caso de las bestias, ataca a todos los viajeros.
- **User:** afecta al personaje que utilizó la habilidad.
- **Ally:** selecciona un personaje entre los aliados, pudiendo seleccionar a la misma unidad que utilizó la habilidad. Estas habilidades se pueden seleccionar solo sobre unidades vivas, con la única excepción de las habilidades que tienen el efecto de revivir.
- **Party:** afecta a todos los personajes aliados, incluyendo al personaje que utilizó la habilidad.
- **Any:** puede seleccionar tanto a un aliado, a sí mismo o a una bestia.

### 7.2. Efectos

Toda habilidad tiene al menos un efecto. Para el caso de las habilidades activas y de bestias, estos se producen por utilizar la habilidad como la acción del turno, mientras que para las habilidades pasivas los efectos se activarán si se cumplen las condiciones de la habilidad.

#### 7.2.1. Aumento de stat durante el combate

Este efecto agrega un valor a la *stat* base de la unidad que porta la habilidad. Este efecto dura todo el juego.

#### 7.2.2. Daño

Este efecto genera daño de tipo físico o elemental, dependiendo de la descripción de la habilidad, hacia la unidad que se dirige el ataque.

Los tipos de daño pueden ser:

- `Sword`
- `Spear`
- `Axe`
- `Dagger`
- `Bow`
- `Stave`
- `Fire`
- `Ice`
- `Lightning`
- `Wind`
- `Light`
- `Dark`

Estos ataques aportan como un golpe para la mecánica de **Breaking Point**.

El cálculo del daño se realiza de la misma manera que los ataques básicos, donde el modificador se obtiene de la descripción de la habilidad.

#### 7.2.3. Recuperación

Este efecto restaura HP o SP a la unidad objetivo, la cual necesariamente debe estar viva.

En general, la cantidad restaurada se calcula según la *stat* **Elem Def** de la unidad que utiliza la habilidad:

```text
Heal = ⌊Elem Def × M⌋
```

donde `M` corresponde al valor **Modificador** de la habilidad utilizada.

#### 7.2.4. Revivir

Este efecto devuelve a la vida a un viajero caído. Al revivir la unidad siempre se le dará **1 HP**.

Cuando una unidad revive, podrá actuar **a partir de la siguiente ronda**, no en la ronda en la que revive.

#### 7.2.5. Estados

Este tipo de efectos se aplica por cierta cantidad de tiempo, medido en rondas o acciones, de tal forma que se pueden *stackear* los efectos del mismo tipo.

Si una unidad posee un estado y se le vuelve a aplicar el mismo estado, simplemente se extenderá la duración de su efecto. En ningún caso se alterará o potenciará el efecto del estado.

La duración de los estados empieza a contar desde que se aplica el efecto, independiente de si la unidad que lo recibe actuó o no.

##### Buffs

Este efecto modifica temporalmente las características de la unidad afectada, beneficiándola. No modifica los *stats* base y su duración depende de cada habilidad.

Tipos de buffs:

- **Increased Physical Attack:** al realizar un ataque físico, multiplica el daño a realizar por ×1.5.
- **Increased Elemental Attack:** al realizar un ataque elemental, multiplica el daño a realizar por ×1.5.
- **Increased Physical Defense:** al recibir un ataque físico, multiplica el daño recibido por ×2/3.
- **Increased Elemental Defense:** al recibir un ataque elemental, multiplica el daño recibido por ×2/3.
- **Increased Speed:** al ordenar la cola de turnos, la unidad se posicionará como si tuviera ×1.5 de su Speed base.

##### Debuffs

Este efecto modifica temporalmente las características de la unidad afectada, perjudicándola. Tampoco modifica los *stats* base.

Tipos de debuffs:

- **Decreased Physical Attack:** al realizar un ataque físico, multiplica el daño a realizar por ×2/3.
- **Decreased Elemental Attack:** al realizar un ataque elemental, multiplica el daño a realizar por ×2/3.
- **Decreased Physical Defense:** al recibir un ataque físico, multiplica el daño recibido por ×1.5.
- **Decreased Elemental Defense:** al recibir un ataque elemental, multiplica el daño recibido por ×1.5.
- **Decreased Speed:** al ordenar la cola de turnos, la unidad se posicionará como si tuviera ×2/3 de su Speed base.

Los estados de buff y debuff no modifican directamente los *stats* de las unidades. Modifican el daño infligido/recibido o la posición que tomarán en la cola de turnos.

##### Ailments

Este efecto otorga un estado perjudicial que permanece por cierta cantidad de rondas.

Tipos de *ailments*:

- **Poison:** la unidad recibe daño igual al 17 % de su HP máximo luego de actuar en su turno. Si la unidad no tuvo turno en una ronda, entonces se aplica el daño al final de la ronda. Si la unidad tiene más de un turno por ronda, recibirá daño después de cada uno de sus turnos.
- **Silence:** la unidad no puede utilizar habilidades activas.
- **Unconscious:** la unidad pierde su turno durante la ronda, de modo que no participa en ella a pesar de estar viva.
- **Sleep:** la unidad pierde el turno durante la ronda, de modo que no participa en ella a pesar de estar viva. El estado se remueve por completo si la unidad recibe daño por un ataque.
- **Terror:** la unidad no puede utilizar *boost* y tampoco ganará BP al final de la ronda.

##### Priorización en cola de turnos

Este efecto le otorga a la unidad prioridad en la cola de turnos. Como se explicó en el algoritmo, una unidad con prioridad en la cola se posiciona antes que el resto ignorando su velocidad.

No existen habilidades que le otorguen prioridad a una bestia, solo a viajeros.

##### Despriorización en cola de turnos

Este efecto provoca que la unidad se posicione al final de la cola de turnos, ignorando su velocidad.

No existen habilidades que desprioricen el turno de un viajero, solo a bestias.

##### Mejoras

Este efecto otorga mejoras a la unidad similares a los buffs, pero su duración no se mide por rondas, sino por acciones o usos.

Tipos de mejoras:

- **CounterAttack:** refleja un ataque físico, infligiendo su daño a la unidad atacante.
- **ReflectiveVeil:** refleja un ataque elemental, infligiendo su daño a la unidad atacante.

Estos efectos no tienen duración por rondas. La unidad no perderá el efecto hasta que lo utilice. Por ejemplo, si una unidad tiene `CounterAttack` por 2 acciones, conservará el efecto hasta que reciba un ataque físico. En ese momento reflejará el daño y pasará a tener `CounterAttack` por 1 acción.

##### Combinaciones

Una unidad puede tener múltiples estados de forma simultánea, aplicando cada uno de los estados para obtener los efectos finales.

Por ejemplo, una unidad con **Increased Physical Attack** y con **Decreased Physical Attack** calculará su daño de la forma:

```text
Daño final = Daño base × 1.5 × 2/3
```

#### 7.2.6. Especiales

En esta categoría entran los efectos que no caen directamente en las categorías anteriores. Algunos tipos de efectos especiales son:

- modificar la selección de objetivo;
- aumentar la duración de buffs y debuffs;
- otorgar un turno extra;
- realizar un ataque extra;
- alterar la cantidad de veces que se efectúa la habilidad;
- anular daño recibido;
- infligir más daño;
- entre otros.

### 7.3. Habilidades activas

Forman parte de las acciones que pueden realizar los viajeros. Corresponden a movimientos con distintos efectos, que se aplican sobre distintos personajes. Son más poderosas que un ataque básico, pero a cambio se debe gastar el SP correspondiente.

No tener suficiente SP significa que no se puede utilizar la habilidad.

Las características que definen una habilidad activa son las siguientes:

- **Nombre:** nombre de la habilidad, su identificador único.
- **Costo de SP:** cantidad de SP que el viajero consumirá para utilizar la habilidad.
- **Tipo:** representa el tipo de ataque que tiene la habilidad. Las habilidades que no realizan daño no poseen tipo.
- **Descripción:** texto que define el efecto de la habilidad.
- **Objetivo:** determina sobre quién o quiénes se puede utilizar la habilidad.
- **Modificador:** valor que determina la potencia y efectividad de algunas habilidades.
- **Boost:** descripción de la mejora que recibe la habilidad al ser utilizada con BP.

Ejemplos de habilidades activas:

- **Fireball:** inflige daño de tipo `Fire` a todos los enemigos.
- **Last Stand:** inflige daño de tipo `Axe` a todos los enemigos. Realiza 3 % más de daño por cada 1 % de HP que le falte a la unidad que usa la habilidad.
- **Revive:** revive a todos los aliados caídos con 1 HP.
- **Spearhead:** inflige daño de tipo `Spear` a un enemigo y posiciona al usuario primero en la cola de turnos de la siguiente ronda.
- **Lion Dance:** otorga `Increased Physical Attack` a un aliado durante 2 rondas.
- **Reflective Veil:** otorga a un aliado `ReflectiveVeil` por 1 acción.
- **Steal SP:** realiza dos ataques del tipo `Dagger` a un enemigo y restaura SP equivalente al 5 % del daño realizado.
- **Incite:** durante 3 rondas, el usuario se vuelve el objetivo de las habilidades rivales que afectan a un único personaje.

Una habilidad puede tener 1 o más efectos de los descritos anteriormente. Siempre que se tenga la cantidad necesaria de SP para ejecutar la habilidad, esta se podrá usar como una acción en el turno.

#### 7.3.1. Boost en habilidades

Las habilidades pueden verse beneficiadas al ser usadas con **BP**, adquiriendo un efecto más potente. La mejora que recibe una habilidad al ser utilizada con *boosting* dependerá de la descripción de la propia habilidad.

##### Habilidades con efecto de daño

Habilidades que tienen como único efecto el daño a un enemigo aumentan su modificador por cada BP utilizado.

Ejemplo con `Fireball`:

- **Descripción:** inflige daño de tipo `Fire` a todos los enemigos.
- **Modificador:** 1.5
- **Boost:** aumenta el modificador en un 90 % del modificador base por cada BP gastado.

Si se utiliza `Fireball` con 2 BP:

```text
Modificador = 1.5 + (0.9 × 2) × 1.5 = 4.2
```

##### Habilidades con efecto de recuperación

Las habilidades que se centran en restaurar HP o SP se ven beneficiadas aumentando su modificador por cada BP utilizado.

Ejemplo con `Heal Wounds`:

- **Descripción:** restaura HP a todas las unidades aliadas.
- **Modificador:** 1.5
- **Boost:** aumenta el modificador en 0.5 por cada BP gastado.

Si se utiliza `Heal Wounds` con 2 BP:

```text
Modificador = 1.7 + 0.5 × 2 = 2.7
```

##### Habilidades de estado

Las habilidades que otorgan buffs, debuffs o ailments aumentan la duración del efecto por cada BP utilizado.

Ejemplo con `Lion Dance`:

- **Descripción:** otorga `Increased Physical Attack` a un aliado por 2 rondas.
- **Boost:** aumenta la duración en 2 rondas por cada BP.

Si se utiliza `Lion Dance` con 3 BP:

```text
Duración = 2 + (2 × 3) = 8 rondas
```

De forma similar, habilidades que otorgan mejoras aumentarán la duración por cada BP utilizado, pero medido en acciones y no en rondas.

Ejemplo con `Moon’s Reflexion`:

- **Descripción:** otorga a un aliado `CounterAttack` por 1 acción.
- **Boost:** aumenta la duración en 1 acción por cada BP.

Si se utiliza `Moon’s Reflexion` con 2 BP:

```text
Duración = 1 + (1 × 2) = 3 acciones
```

#### 7.3.2. Habilidades divinas

Estas habilidades son habilidades activas especiales, ya que solo se pueden utilizar cuando se tienen al menos **3 BP**. A diferencia del resto de habilidades activas, estas no tienen un efecto potenciado por utilizar *boosting*, porque se requiere del boosting para poder accionarlas.

Ejemplos de habilidades divinas:

- **Aelfric’s Auspices:** selecciona a un aliado. Durante 3 rondas, las habilidades de ese aliado se activarán 2 veces. No afecta a las habilidades divinas.
- **Alephan’s Enlightenment:** selecciona a un aliado. Durante 3 rondas, sus habilidades con *target* `Enemies` tendrán *target* `Single`, realizando el doble de daño. No afecta a las habilidades divinas.
- **Steorra’s Prophecy:** inflige daño de tipo `Dark` a todos los enemigos. Aumenta el modificador en un 20 % por cada BP que tenga el equipo.
- **Winnehild’s Battle Cry:** inflige daño de las 6 armas (`Sword`, `Spear`, `Dagger`, `Axe`, `Bow` y `Stave`) a todos los enemigos.

Estas habilidades se componen principalmente de efectos especiales, donde pueden cambiar la forma de ejecución, los objetivos o la forma de calcular el daño. Son habilidades poderosas que consumen más SP que el resto.

Al igual que los efectos de estado, habilidades como **Aelfric’s Auspices** o **Alephan’s Enlightenment** pueden *stackearse*. Por ejemplo, si una unidad ya tiene el efecto de `Aelfric’s Auspices` y otro viajero usa `Aelfric’s Auspices` en la misma unidad, entonces el efecto durará 6 rondas en vez de 3.

No obstante, estas habilidades en ningún caso alterarán la ejecución de otra habilidad divina. Si una unidad está bajo el efecto de `Aelfric’s Auspices` y usa `Winnehild’s Battle Cry`, esta no se activará 2 veces.

### 7.4. Habilidades pasivas

Las habilidades pasivas son habilidades que otorgan distintos efectos en favor del usuario, que pueden afectar tanto a la unidad como a un aliado o un enemigo. Estas habilidades actúan de forma pasiva durante todo el combate y el jugador no puede elegir si se usan o no.

Cada habilidad pasiva tiene los siguientes atributos:

- **Nombre:** nombre de la habilidad, su identificador único.
- **Descripción:** texto que define el efecto de la habilidad.
- **Objetivo:** determina cuáles son las unidades que pueden recibir el efecto de la habilidad.

Ejemplos de habilidades pasivas:

- **Summon Strengh:** aumenta en 50 el *stat* `Phys Atk` de la unidad que porte la habilidad.
- **Patience:** si al final de la ronda el *stat* HP y el *stat* SP de la unidad son pares, entonces la unidad obtiene un turno adicional antes de finalizar la ronda.
- **The Show Goes On:** los buffs que la unidad otorgue a sus aliados tendrán 1 ronda extra de duración.
- **Cover:** el usuario recibe los ataques con *target* `Single` que se realicen sobre aliados con 30 % de vida o menos.
- **Incidental Attack:** al usar habilidades que no realicen daño sobre un enemigo, si el HP del usuario es impar, realiza un ataque básico contra el objetivo con su primera arma.
- **Physical Prowess:** el portador tendrá `Increased Physical Attack` y `Increased Physical Defense` durante toda la partida.
- **BP Eater:** las habilidades del usuario utilizadas con boost realizan un 50 % de daño adicional.
- **Endure:** si el usuario posee algún *ailment*, tendrá `Increased Physical Defense` e `Increased Elemental Defense` por la duración del ailment.

Los efectos de las habilidades pasivas se activarán siempre y cuando se cumpla la condición de la misma. Hay habilidades que no poseen condiciones, como `Summon Strengh`, y por tanto se activarán siempre.

Un viajero muerto no puede hacer uso de sus habilidades pasivas.

### 7.5. Habilidades de las bestias

Las bestias, a diferencia de los viajeros, solo tienen la acción **Usar Habilidad**. Sin embargo, sus habilidades son similares a las habilidades activas de los viajeros.

Cada bestia posee una única habilidad y, dado que las bestias no poseen elección de objetivo, este se obtendrá de la definición de la habilidad.

Ejemplos de habilidades de bestias:

- **Attack:** realiza un ataque físico al viajero con mayor HP.
- **Incinerate:** realiza un ataque elemental a todos los viajeros.
- **Double Bite:** realiza un ataque físico al viajero con menor `Phys Def` dos veces.
- **Acid Spray:** otorga `Decreased Physical Defense` y `Decreased Elemental Defense` al viajero con mayor HP durante 2 rondas.
- **Poison Strike:** realiza un ataque físico al viajero con mayor HP. Adicionalmente aplica `Poison` por 2 rondas.
- **Vortal Claw:** realiza un ataque que reduce a la mitad el HP de todos los viajeros.

Al igual que las habilidades activas, estas pueden tener 1 o más efectos que se activan simplemente al utilizar la habilidad.

---

## Resumen general

Este enunciado define una versión simplificada del sistema de combate por turnos de *Octopath Traveler*. El juego enfrenta a un equipo de viajeros, controlado por el jugador, contra un equipo de bestias, controlado automáticamente. La lógica del combate se organiza en rondas y turnos, donde la velocidad, los efectos de habilidades, el uso de **Defender**, el gasto de **BP**, y especialmente el sistema de **Shields** y **Breaking Point**, determinan el orden y el impacto de las acciones.

Las unidades comparten varios *stats* básicos, pero se distinguen en elementos clave: los viajeros poseen armas, SP, habilidades activas y pasivas; las bestias poseen una única habilidad, debilidades y Shields. El combate combina ataques físicos y elementales, con reglas explícitas para calcular daño, aplicar multiplicadores por debilidad o ruptura, truncar decimales y manejar estados temporales.

El documento también formaliza una arquitectura de habilidades basada en objetivos y efectos. Las habilidades pueden dañar, curar, revivir, aplicar buffs, debuffs, ailments, mejoras o efectos especiales. Además, especifica cómo funciona el *boosting* de habilidades, cómo operan las habilidades divinas y cómo se activan las habilidades pasivas. En conjunto, el enunciado entrega una base detallada para implementar un motor de combate con reglas claras, testeables y suficientemente ricas como para capturar buena parte de la identidad táctica de *Octopath Traveler*.
