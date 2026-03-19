# Reglas de Clean Code extraídas de los PDFs

## Principio general
- Escribe código que otra persona pueda entender fácilmente.

## Nombres

### Reglas generales para nombres
- Elige nombres precisos, no ambiguos e intuitivos.
- Si un nombre necesita un comentario para entenderse, el nombre es malo.
- Un buen nombre debe facilitar la comprensión del código.
- Un mal nombre no dice nada o induce a pensar algo incorrecto.

### 1. Usa nombres que revelen intención
- El nombre de una variable, función o clase debe dejar claro:
  - por qué existe,
  - qué hace,
  - cómo se usa.
- El nombre debe indicar qué se está representando y, cuando corresponda, en qué unidades.

### 2. Evita la desinformación
- No uses nombres que puedan interpretarse de forma incorrecta.
- No uses nombres que sugieran una estructura o tipo que no corresponde.
- No uses nombres demasiado parecidos cuando hacen cosas distintas.
- Escribe conceptos similares de forma similar.
- No seas inconsistente al nombrar cosas equivalentes.

### 3. Haz distinciones significativas
- No distingas nombres solo porque el compilador lo exige.
- No uses sufijos o diferencias triviales como números para distinguir entidades.
- No uses sinónimos vacíos para aparentar diferencias donde no las hay.
- Toda diferencia entre nombres debe corresponder a una diferencia real de significado.

### 4. Usa nombres buscables
- Prefiere nombres fáciles de encontrar mediante búsqueda.
- Evita nombres de una sola letra cuando el identificador no es trivial.
- Evita números sueltos o literales mágicos en lugar de nombres con significado.

### 5. Usa sustantivos para clases y verbos para métodos
- Las clases, variables, atributos y objetos deben nombrarse con sustantivos o frases sustantivas.
- Los métodos deben nombrarse con verbos o frases verbales.
- No nombres clases con verbos.
- No nombres métodos con sustantivos.

### 6. Usa una palabra por concepto
- Elige una única palabra para cada concepto abstracto y úsala consistentemente.
- No alternes palabras equivalentes para la misma operación o responsabilidad.

### 7. No reutilices la misma palabra para propósitos distintos
- No uses la misma palabra para representar conceptos distintos.
- Un mismo término no debe significar cosas diferentes según el contexto.

### 8. Usa nombres del dominio de la solución cuando corresponda
- Usa términos computacionales estándar para algoritmos, patrones, estructuras de datos y conceptos técnicos.

### 9. Usa nombres del dominio del problema cuando corresponda
- Si no existe un término técnico estándar adecuado, usa nombres del dominio real del problema.

### 10. Agrega contexto significativo
- Sitúa los nombres dentro de clases, funciones o namespaces con buen nombre.
- Agrega contexto al identificador solo cuando el nombre, por sí solo, sea ambiguo.

### 11. No agregues contexto gratuito
- Prefiere nombres cortos cuando siguen siendo claros.
- No agregues prefijos o contexto redundante si el contexto ya está dado por la estructura que contiene al nombre.

## Funciones

### 1. Mantén las funciones pequeñas
- Idealmente, una función debería tener entre 4 y 5 líneas.
- Una función debería tener como máximo uno o dos niveles de indentación.
- Una función debe tener un nombre apropiado.

### 2. Las funciones deben hacer una sola cosa
- Una función debe hacer una sola cosa.
- Debe hacerla bien.
- Debe hacer solo esa cosa.
- Si una función mezcla varias tareas, divídela.

### 3. Mantén un único nivel de abstracción por función
- Los pasos internos de una función deben estar un nivel por debajo del nombre de la función.
- No mezcles detalles de bajo nivel con pasos de alto nivel dentro de la misma función.

### 4. Extrae funciones hasta que tenga sentido detenerse
- Para lograr que una función haga una sola cosa, sigue extrayendo subfunciones mientras eso mejore la claridad.

### 5. Separa comandos de consultas
- Una función debe hacer algo o responder algo, pero no ambas cosas a la vez.
- No combines modificación de estado con retorno de información en la misma operación.

### 6. Minimiza la cantidad de argumentos
- Cero argumentos es lo ideal.
- Uno es mejor que dos.
- Tres casi nunca.
- Nunca más de tres.

### 7. Reduce argumentos cuando sea posible
- Convierte argumentos en atributos cuando pertenecen naturalmente al estado del objeto.
- Agrupa argumentos relacionados dentro de un objeto apropiado.
- Si un argumento es un flag booleano, sepáralo en métodos distintos.

### 8. No uses output arguments
- No uses parámetros cuyo propósito sea devolver resultados por referencia o mutación externa.

### 9. No mezcles cambio de estado y consulta
- Una función debe cambiar el estado interno de un objeto o retornar información sobre él, no ambas cosas.

### 10. Evita código duplicado
- Si un bloque de código aparece repetido, extráelo.
- No repitas lógica que puede vivir en una función común.

### 11. Usa excepciones en lugar de códigos de error
- Prefiere lanzar excepciones antes que retornar códigos de error.

### 12. Separa manejo de errores de control de flujo
- El manejo de errores es una tarea.
- El control de flujo es otra tarea.
- No mezcles ambas responsabilidades dentro de la misma función.

## Versión condensada para usar como base de un SKILL
- Prioriza legibilidad sobre brevedad engañosa.
- Usa nombres precisos, consistentes, buscables y con contexto suficiente.
- Usa sustantivos para entidades y verbos para acciones.
- Mantén funciones pequeñas, con poca indentación y una sola responsabilidad.
- No mezcles comando con consulta.
- Minimiza argumentos y elimina flags booleanos.
- Evita duplicación.
- Prefiere excepciones sobre códigos de error.
- No mezcles niveles de abstracción dentro de una misma función.


## Apéndice opcional: convenciones de nombres mostradas para C#
- `private field`: `lowerCamelCase`
- `public field`: `UpperCamelCase`
- `protected field`: `UpperCamelCase`
- `internal field`: `UpperCamelCase`
- `property`: `UpperCamelCase`
- `method`: `UpperCamelCase`
- `class`: `UpperCamelCase`
- `interface`: `IUpperCamelCase`
- `local variable`: `lowerCamelCase`
- `parameter`: `lowerCamelCase`
