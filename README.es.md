# create-agent-project

*(🇬🇧 [Read in English](README.md))*

## ¿Por qué importa esto?

Hoy la mayoría de la gente usa la IA como un chat: le preguntás algo,
te responde, listo. Pero el salto grande — trabajar con la IA de forma
**agéntica**, donde no solo responde sino que lee tus archivos,
entiende el contexto de tu proyecto, y va avanzando trabajo real por
vos — hoy lo están aprovechando casi exclusivamente los programadores,
porque las herramientas para hacerlo (Claude Code, OpenCode, Codex CLI)
nacieron pensadas para código.

Esta herramienta no tiene esa limitación. Le sirve exactamente igual a
un escritor, a un diseñador, a alguien haciendo una investigación, que
a un programador — el modo de trabajo agéntico deja de ser algo
exclusivo de quien sabe programar.

## ¿Qué es esto, en criollo?

Es una herramienta gratuita y de código abierto que te arma, en un par
de minutos, **la carpeta y los archivos de partida** para empezar un
proyecto trabajando con inteligencia artificial (Claude Code, OpenCode,
Codex CLI, o lo que uses) — sin que vos tengas que saber nada de
programación, de "agentes" ni de IA en cuanto al *contenido*: le
contás en lenguaje normal qué querés hacer, y ella arma la estructura
de archivos y carpetas correcta para eso — ni más ni menos de lo que
tu proyecto necesita.

**Importante y honesto:** hoy esto es una herramienta de consola (ver
"Roadmap" más abajo). Para usarla hoy hace falta saber abrir una
terminal y correr un comando — eso sí es un paso técnico real, y no
te lo vamos a esconder. La idea final (una web sin terminal, para
cualquiera) todavía no está lista.

## ¿Para quién es hoy, en esta versión?

Para alguien que **ya sabe usar una terminal** — programador o no —
trabajando solo en lo que sea: código, un libro, una investigación,
un proyecto de diseño, un plan de negocio. No hace falta saber nada
de "arquitectura de agentes" ni de IA para usarla, solo saber correr
un comando.

Si nunca usaste una terminal, todavía no es para vos — pero la versión
que sí lo va a ser está en camino (ver "Roadmap").

## ¿Qué NO es?

No es una plataforma, no es un framework de IA, no ejecuta nada por su
cuenta. Solo genera la estructura inicial — el "cómo arrancar bien" —
y después vos trabajás directamente con tu herramienta de IA favorita
sobre esa estructura.

## Cómo se usa

1. Descargás el programa (no hace falta instalar nada más) desde
   [Releases](https://github.com/juanfarcik/create-agent-project/releases)
2. Lo corrés y contestás unas preguntas simples sobre tu proyecto
3. Te genera una carpeta lista
4. Abrís esa carpeta con Claude Code, OpenCode, Codex CLI (o la que
   uses) y empezás a hablarle en lenguaje normal

Instrucciones técnicas completas (en inglés): [`dotnet/README.md`](dotnet/README.md).

## Roadmap

**Lo que hay hoy** es el código fuente y la versión de consola — una
herramienta de línea de comandos real, para quien ya está cómodo con
una terminal (programador o no).

**Lo que viene después** es una versión web simple, sin terminal,
pensada directamente para el público que en el fondo nos importa
alcanzar: escritores, diseñadores, investigadores, cualquiera con un
proyecto y cero conocimiento de programación. La consola no es una
opción de segunda que se va a reemplazar — es el motor que la web va
a usar por debajo (ver la sección "Api seam" en
[`dotnet/docs/ARCHITECTURE.md`](dotnet/docs/ARCHITECTURE.md)). Las dos
van a seguir existiendo juntas.

## Sobre el proyecto

Es un proyecto personal, de código abierto (licencia GPLv3), hecho
como investigación abierta — no es un producto de una empresa. *(El
autor también trabaja profesionalmente en orquestación de agentes para
equipos de ingeniería — un proyecto comercial aparte, sin relación con
este repositorio.)*

Toda decisión de diseño está documentada y con fuente real — nada
inventado ni exagerado. Ver [`dotnet/docs/REFERENCES.md`](dotnet/docs/REFERENCES.md)
si te interesa el detalle.

## Licencia y participación

GPLv3 — ver [LICENSE](LICENSE). Si querés contribuir, sos bienvenido:
ver [`dotnet/CONTRIBUTING.md`](dotnet/CONTRIBUTING.md) (en inglés).
