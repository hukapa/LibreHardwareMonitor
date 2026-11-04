# Relatorio Tecnico — Estado do Projeto e Melhorias Implementadas

> **Projeto:** LibreHardwareMonitor (Fork hukapa)  
> **Data:** Novembro de 2026  
> **Autor:** Antigravity & hukapa  
> **Estado da Solucao:** Compilacao com 0 Erros e 0 Avisos Criticos em .NET 8.0 LTS (`net8.0-windows`)  
> **Regra Fundamental:** Zero Emojis  

---

## 1. Sumario Executivo

Este documento consolida todas as investigacoes tecnicas, correcoes arquiteturais e melhorias de usabilidade aplicadas no fork do LibreHardwareMonitor. O projeto foi atualizado a partir do upstream, adaptado para o ecossistema moderno do .NET 8.0 LTS, blindado contra politicas de seguranca avancadas do Windows 11 (Smart App Control), modularizado em classes parciais orientadas a responsabilidade unica e equipado com uma interface web de monitorizacao remota contemporanea.

---

## 2. Diagnostico e Resolucao do Bloqueio no Windows 11 (Smart App Control)

### 2.1 Identificacao da Causa Raiz
Ao tentar iniciar a aplicacao no Windows 11 com o Controlo de Aplicacoes Inteligente (Smart App Control - SAC) ativo (`VerifiedAndReputablePolicyState: 1`), o processo encerrava silenciosamente logo apos a confirmacao da caixa de dialogo do UAC.

A inspecao profunda dos registos do Windows (`Microsoft-Windows-CodeIntegrity/Operational` e `Application Error`) revelou dois fatores criticos:
1. **Bloqueio de DLLs nao assinadas:** A biblioteca `RAMSPDToolkit-NDD.dll` e posteriormente `Aga.Controls.dll` eram bloqueadas pelo kernel do Windows com a excecao:
   ```text
   System.IO.FileLoadException: Could not load file or assembly 'RAMSPDToolkit-NDD.dll'.
   Uma política de Controlo de Aplicações bloqueou este ficheiro. (0x800711C7)
   ```
2. **Falha de Elevacao Silenciosa (Erro 740):** O manifest original continha `<requestedExecutionLevel level="requireAdministrator" />`. Quando invocado atraves de uma consola ou script sem elevacao prévia, a API `CreateProcess` falhava imediatamente com `ERROR_ELEVATION_REQUIRED` (traduzido pelo PowerShell como "O pedido nao e suportado").

### 2.2 Medidas Corretivas Implementadas
1. **Isolamento de Carregamento Tardio (`NoInlining`):**  
   Em `MemoryGroup.cs`, a chamada ao `RAMSPDToolkit` foi movida para o metodo privado `InitializeSpd()`, anotado com `[MethodImpl(MethodImplOptions.NoInlining)]`. Desta forma, o compilador JIT do CLR nao tenta carregar a DLL ao instanciar o `MemoryGroup`. Se o Windows bloquear o driver de leitura SPD, a excecao e capturada no bloco `try / catch` sem interromper a aplicacao.
2. **Blindagem Global de Grupos de Hardware:**  
   Em `Computer.cs`, a instanciacao de todos os grupos de hardware foi envolvida em blocos de protecao para evitar que qualquer driver de terceiros provoque um encerramento inesperado.
3. **Captura Global de Excecoes e Registo em `crash.log`:**  
   Em `Program.cs`, foram subscritos os eventos `AppDomain.CurrentDomain.UnhandledException` e `Application.ThreadException`. Qualquer erro fatal grava agora o stacktrace detalhado em `crash.log` e exibe uma caixa de dialogo informativa ao utilizador.
4. **Ajuste do Manifest para `asInvoker`:**  
   Em `app.manifest`, o nivel de execucao foi ajustado para `asInvoker`. O executavel pode agora ser chamado sem exigir elevacao forçada do sistema, enquanto o script `Iniciar-LibreHardwareMonitor.bat` assegura elevacao limpa via `-Verb RunAs` e define explicitamente o `-WorkingDirectory`.
5. **Estrategia Single-File Standalone:**  
   Foi disponibilizada a opcao de publicacao em ficheiro unico (`PublishSingleFile=true`), onde todas as DLLs dependentes sao embutidas diretamente dentro do binario `LibreHardwareMonitor.Windows.Forms.exe`. Como nao existem ficheiros DLL soltos no disco, o Smart App Control nao bloqueia os modulos managed em tempo de execucao.

---

## 3. Minimizar para Segundo Plano ao Clicar no "X"

### 3.1 Objetivo
Permitir que o utilizador feche a janela principal sem terminar o servico de monitorizacao, mantendo o processo ativo na bandeja do sistema (System Tray) e o servidor web funcional.

### 3.2 Implementacao Tecnica
* Foi adicionada a opcao `_minimizeOnClose` (ativa por omissao).
* O metodo `OnFormClosing(FormClosingEventArgs e)` verifica se o motivo de encerramento foi iniciado pelo utilizador (`CloseReason.UserClosing`). Se verdadeiro, cancela o encerramento (`e.Cancel = true`), oculta a janela (`Visible = false`) e assegura que o icone do System Tray fica visivel.
* No `WndProc`, a intercepcao das mensagens `WM_SYSCOMMAND` com parametro `SC_CLOSE` redireciona para a rotina `SysTrayHideShow()`.
* O encerramento definitivo e agora executado atraves do menu contextual da bandeja ("Exit") ou do menu principal, invocando o metodo `CloseApplication()`.

---

## 4. Modernizacao do Dashboard Web Remoto

### 4.1 Problema do Legado
A interface web anterior remontava a 2012, baseada em versoes obsoletas do jQuery e Knockout.js, com dependencias externas nao manutenidas e estetica desatualizada.

### 4.2 Nova Arquitetura (`Resources/Web/index.html`)
* **Zero Dependencias:** 100% desenvolvido em HTML5 semantico, CSS moderno e JavaScript nativo (ES2022).
* **Suporte Completo a Dark/Light Mode:** Utilizacao da propriedade CSS `color-scheme: light dark` e da funcao nativa `light-dark()`, respeitando a preferencia de cores do sistema operativo do utilizador.
* **Layout em Gridcards Responsivo:** Os sensores sao agrupados visualmente por componente de hardware em cartoes limpos com animacoes suaves.
* **Pesquisa e Filtragem em Tempo Real:** Barra de pesquisa instantanea no topo da pagina para localizar rapidamente qualquer sensor de temperatura, voltagem ou utilizacao.
* **Consumo Eficiente da API:** Polling assincrono para `/data.json` com re-renderizacao otimizada do DOM e indicador visual de estado de ligacao em direto.

---

## 5. Decomposicao Modular de `MainForm.cs`

A classe `MainForm.cs` continha originalmente 1.479 linhas, violando o Principio da Responsabilidade Unica (SRP) e dificultando a manutencao. Foi modularizada em 5 classes parciais (`sealed partial class MainForm`), todas alinhadas com o padrao do projeto CrystalStudio AI:

```text
LibreHardwareMonitor.Windows.Forms/UI/
├── MainForm.cs            # ~700 lin: Construtor, ciclo de vida, ciclo de atualizacao, definicoes e estado
├── MainForm.Tray.cs       # ~105 lin: System Tray, minimizar para segundo plano, intercepcao WndProc e saida
├── MainForm.WebServer.cs  # ~ 40 lin: Controlo do servidor HTTP, configuracao de porta e credenciais
├── MainForm.Plot.cs       # ~240 lin: Painel de graficos, modelos temporais OxyPlot e selecao de sensores
└── MainForm.Tree.cs       # ~270 lin: Gestao da arvore de sensores TreeViewAdv, eventos de rato e menus de contexto
```

---

## 6. Estado Atual de Compilacao e Testes

* **Framework de Destino Principal:** .NET 8.0 Windows (`net8.0-windows`)
* **Compatibilidade Retroativa Mantida:** .NET Framework 4.7.2 (`net472`)
* **Erros de Compilacao:** 0
* **Avisos:** 0 avisos bloqueantes
* **Regra Zero Emojis:** 100% em conformidade em todo o repositorio.
