Realización de un portal académico que permite la matrícula de estudiantes
Stack: ASP.NET Core MVC (.NET 8), Identity, EF Core, Razor Views, Caché/Sesiones: Redis local.
Base de datos: SQLite local.

El desarrollo de la pregunta 1 se encuentra en la rama -- feature/bootstrap-dominio
El desarrollo de la pregunta 2 se encuentra en la rama -- feature/catalogo-cursos
El desarrollo de la pregunta 3 se encuentra en la rama -- feature/matriculas
El desarrollo de la pregunta 4 se encuentra en la rama -- feature/sesion-redis
El desarrollo de la pregunta 5 se encuentra en la rama -- feature/panel-coordinador

Se realizó la asignación de roles con las siguientes cuentas:
Coordinador:
  Email: coordinador@uni.edu
  Contraseña: Admin123!

Estudiantes:
  estudiante1@uni.edu / Password123!
  estudiante2@uni.edu / Password123!

Es necesario iniciar cuenta para realizar las acciones.
Al iniciar cuenta se empieza con el listado de 3 cursos iniciales, se realizaron los impedimentos al matricurlase según el documento entregado. 
De igual forma, se colocó la parte de filtros para tener una búsqueda más fluida.
En cuanto al coordinador, es posible que agregue nuevos cursos y de igual forma editar la información de estos, los cambios realizados se visualizan de igual forma en la base de datos.
