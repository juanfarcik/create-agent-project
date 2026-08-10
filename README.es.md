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
programación, de "agentes" ni de IA.

Vos le contás en lenguaje normal qué querés hacer. La herramienta te
arma la estructura de archivos y carpetas correcta para eso — ni más
ni menos de lo que tu proyecto necesita — y te queda lista para abrir
con tu herramienta de IA y arrancar a trabajar de una.

## ¿Para quién es?

Para **una persona sola, trabajando en lo que sea** — no es para
empresas ni equipos:

- Un **escritor** armando un libro, un blog, un guion
- Un **diseñador** armando un proyecto visual o de marca
- Alguien haciendo **investigación** — de mercado, académica, lo que sea
- Un **músico o artista** armando un proyecto creativo
- Alguien con una **idea de negocio** que quiere ordenarla
- Y sí, también programadores armando software

No hace falta saber nada de "arquitectura de agentes" ni de IA para
usarla — la herramienta te hace unas pocas preguntas simples ("¿qué
querés lograr?", "¿cómo sabés que está terminado?") y arma todo por
vos.

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
