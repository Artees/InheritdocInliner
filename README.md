The `<inheritdoc/>` XML comments tag allows documentation to be inherited from
base types/members, which reduces the effort required to write documentation.
However, some tools don't support this feature. InheritdocInliner is a
command-line tool that postprocesses XML files and replaces `<inheritdoc/>`
elements with actual comments.

# Installation
Install the [NuGet package](https://www.nuget.org/packages/InheritdocInliner/)
or download the latest
[EXE from GitHub](https://github.com/Artees/InheritdocInliner/releases).

# Usage
`path\to\InheritdocInliner.exe path\to\docs.xml`

Command line arguments:

| Argument                   | Description                                                                                                             |
| -------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| --help                     | Display the help screen.                                                                                                |
| --version                  | Display version information.                                                                                            |
| -x, --xml (pos.&nbsp;0)    | The XML document to be processed.                                                                                       |
| -d, --dll (pos.&nbsp;1)    | The assembly file for which the XML document is processed. If not specified, the path of the XML document will be used. |
| -o, --output (pos.&nbsp;2) | The path of the output XML document. If not specified, the original document will be overwritten.                       |
