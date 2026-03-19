# Clean Code: reglas extraídas del Capítulo 6

## Objetos y estructuras de datos

### 1. No expongas detalles internos de los datos
- No expongas atributos internos solo porque sí.
- No agregues getters y setters automáticamente.
- Expón los datos en términos de abstracciones, no en términos de representación interna.
- Prefiere interfaces que expresen intención o significado de negocio antes que interfaces que revelen detalles de almacenamiento o representación.

### 2. Piensa cuidadosamente qué dejas público
- Todo atributo o método público reduce la libertad de cambiar la implementación en el futuro.
- Trata que todo sea privado por defecto.
- Haz público solo lo que realmente forme parte de la abstracción que la clase ofrece.

### 3. Distingue correctamente entre objeto y estructura de datos
- **Estructura de datos**: expone sus datos; si tiene funciones, estas son simples y se enfocan en obtener o editar esos datos.
- **Objeto**: esconde sus datos detrás de abstracciones y expone comportamiento que opera sobre esos datos.
- No mezcles ambos modelos en una misma clase.
- Evita clases híbridas que exponen datos y a la vez concentran comportamiento complejo.

## Anti-simetría entre datos y objetos

### 4. Elige orientación a objetos cuando el sistema cambia agregando nuevos tipos
- Usa objetos, herencia y polimorfismo cuando esperas agregar nuevas clases o nuevos tipos de entidad con frecuencia.
- En este enfoque, agregar un nuevo tipo debe requerir agregar una nueva clase, sin modificar la lógica existente.

### 5. Elige estructuras de datos y código procedural cuando el sistema cambia agregando nuevas funciones
- Usa estructuras de datos simples con funciones externas cuando esperas agregar nuevas operaciones sobre un conjunto relativamente fijo de tipos.
- En este enfoque, agregar una nueva función debe requerir agregar una nueva función, sin modificar las estructuras de datos existentes.

### 6. No asumas que herencia y polimorfismo siempre son la mejor solución
- Herencia y polimorfismo no son automáticamente la respuesta correcta.
- La decisión depende de qué cambia más en el sistema:
  - si cambian más los tipos, favorece OOP;
  - si cambian más las operaciones, favorece estructuras de datos + funciones.

### 7. Evita cadenas de `if` o `switch` que crecen con cada nuevo tipo cuando el dominio favorece polimorfismo
- Si agregar un nuevo tipo obliga a editar un método central lleno de condicionales, considera mover el comportamiento a las clases correspondientes.
- Prefiere un diseño extensible donde el código principal no deba modificarse al introducir nuevos tipos de comportamiento.

## Ley de Demeter

### 8. Un método debe hablar solo con sus colaboradores directos
Un método de una clase debe invocar métodos solo de:
- la misma clase;
- un objeto creado dentro del mismo método;
- un objeto recibido como argumento;
- un objeto almacenado en un atributo de la clase.

### 9. No dependas de la estructura interna de otros objetos
- Un módulo no debe conocer las entrañas de los objetos que manipula.
- Un objeto no debe exponer su estructura interna a través de accessors que obliguen a otros módulos a navegar por ella.
- Si un cliente necesita recorrer internamente otro objeto para obtener algo, falta una abstracción en el objeto proveedor.

### 10. Evita los `train wrecks`
- Evita cadenas largas de llamadas encadenadas, por ejemplo `a.b().c().d()`.
- Estas cadenas indican que un módulo sabe demasiado sobre la estructura interna de otros módulos.
- Si aparece un `train wreck`, elimínalo como mínimo usando variables auxiliares.
- Idealmente, reemplázalo por un método de más alto nivel en el objeto apropiado.

### 11. Mueve la responsabilidad al objeto correcto para cumplir Demeter
- Si una clase rompe la Ley de Demeter para obtener un dato o ejecutar una operación, analiza qué método debería agregarse a la clase colaboradora.
- Prefiere pedir una operación de alto nivel en vez de extraer piezas internas y componer la operación desde fuera.

### 12. La Ley de Demeter aplica a objetos, no a estructuras de datos
- No apliques esta heurística mecánicamente sobre estructuras de datos simples.
- La restricción es relevante cuando trabajas con objetos que deberían ocultar su implementación interna.

## Reglas operativas para diseño limpio

### 13. Diseña para el eje de cambio dominante
- Antes de elegir entre OOP y estructuras de datos, identifica qué va a cambiar más: tipos o funciones.
- Haz que el tipo de cambio más frecuente requiera agregar código nuevo, no modificar código central existente.

### 14. No construyas clases híbridas
- No combines una API pública de datos con lógica de negocio repartida externamente.
- Decide explícitamente si una clase será una estructura de datos simple o un objeto con comportamiento.

### 15. Haz que el código principal escale sin dolor
- Evita diseños que obliguen a modificar el flujo principal cada vez que aparece un nuevo caso.
- Prefiere extensiones por incorporación de nuevas clases o nuevas funciones, según el modelo elegido.

## Checklist breve para usar en un SKILL.md
- Privado por defecto.
- No agregar getters/setters automáticamente.
- Exponer abstracciones, no representación interna.
- No crear clases híbridas.
- Elegir OOP si crecerán los tipos.
- Elegir estructuras de datos + funciones si crecerán las operaciones.
- Evitar cadenas de `if` por tipo cuando corresponde polimorfismo.
- Cumplir Ley de Demeter.
- Evitar `train wrecks`.
- Mover responsabilidades al objeto correcto.
