# LibreHardwareMonitor — Playbook para Agentes de IA e Engenheiros

> **Projeto:** LibreHardwareMonitor (Fork hukapa)  
> **Finalidade:** Guia canonico de normas tecnicas, padroes de arquitetura e convencoes de engenharia para Agentes de IA e programadores.  
> **Regra Suprema:** Zero Emojis em todo o codigo, comentarios, documentacao e mensagens de commit.  

---

## 1. Principios Fundamentais

* **Zero Emojis:** Proibida a utilizacao de emojis em qualquer contexto do repositorio (ficheiros markdown, comentarios em codigo C#, mensagens de commit Git, logs ou textos de interface). Toda a comunicacao deve ser tecnica, concisa e formal.
* **KISS (Keep It Simple, Stupid):** Prefira solucoes diretas e desacopladas. Evite introduzir bibliotecas pesadas de terceiros quando APIs nativas do .NET 8 ou do Windows resolverem o problema com facilidade.
* **DRY (Don't Repeat Yourself):** Reutilize os modelos de nos (`Node`, `HardwareNode`, `SensorNode`) e mecanismos de persistencia (`PersistentSettings`, `UserOption`).
* **Resiliencia Operacional:** A aplicacao pode correr em ambientes com restricoes de seguranca (ex: Windows 11 Smart App Control ativo, utilizadores nao administradores, maquinas virtuais). Nunca assuma que um driver de kernel ou DLL externa de terceiro vai carregar com sucesso.

---

## 2. Limites Rigorosos de Linhas e Estrutura Modular

Para garantir que o codigo permanece legivel e de facil manutencao:
* **Classes C# (.cs):** Limite estrito de **250 a 450 linhas** por ficheiro. Se uma classe crescer alem disso, divida-a em classes parciais funcionais (como demonstrado em `MainForm.cs`, `MainForm.Tray.cs`, `MainForm.WebServer.cs`, `MainForm.Plot.cs`, `MainForm.Tree.cs`) ou desacople servicos auxiliares.
* **Ficheiros Web (.html, .css, .js):** Limite de **300 a 400 linhas**. Evite agrupar estilizacao e logica complexa num unico ficheiro.
* **Principio da Responsabilidade Unica (SRP):** Cada ficheiro deve ter uma unica razao para mudar.

---

## 3. Diretrizes de Interoperabilidade e Resiliencia a Drivers

### 3.1 Isolamento de Tipos Nao Assinados (`NoInlining`)
Sempre que uma funcionalidade depender de uma DLL ou driver de terceiro que possa nao ter assinatura digital de autoridade certificadora publica reconhecida pela Microsoft:
1. Nunca invoque esses tipos diretamente no construtor de uma classe que o CLR precise compilar logo no arranque.
2. Encapsule o carregamento num metodo auxiliar privado com a anotacao:
   ```csharp
   [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
   private void MetodoDeAcessoADriver(...)
   {
       try
       {
           // Instanciacao e uso do driver
       }
       catch (Exception ex)
       {
           // Captura de FileLoadException, DllNotFoundException, etc.
       }
   }
   ```
3. Garanta que, em caso de bloqueio pelo Smart App Control (`0x800711C7`), a aplicacao continua a funcionar com os restantes sensores.

### 3.2 Manifest e Nivel de Privilegios
O manifest principal da aplicacao deve manter:
```xml
<requestedExecutionLevel level="asInvoker" uiAccess="false" />
```
Isto permite que a aplicacao execute normalmente em modo utilizador e possa ser chamada por consolas ou tarefas agendadas sem erro 740 (`ERROR_ELEVATION_REQUIRED`). Elevacao de administrador para leitura de motherboard/CPU deve ser solicitada externamente ou via `Start-Process -Verb RunAs`.

---

## 4. Padroes de Interface e Bandeja (System Tray)

1. **Comportamento de Fecho (Botao "X"):**
   - Ao clicar no "X", se `_minimizeOnClose.Value` estiver ativo, o formulario cancela o fecho (`e.Cancel = true`) e oculta-se para a barra de tarefas (`Visible = false`).
   - Apenas o comando explicito "Exit" (atraves do menu da bandeja ou menu de ficheiro) pode chamar `CloseApplication()` e terminar o processo.
2. **Restauracao da Janela:**
   - Ao reabrir a janela a partir da bandeja, se o estado for `FormWindowState.Minimized`, force `WindowState = FormWindowState.Normal` para assegurar que a janela reaparece no ecrã.

---

## 5. Convencoes de Git e Commits

As mensagens de commit devem seguir o padrao Conventional Commits em portugues ou ingles tecnico, sem emojis:
* `feat:` Novas funcionalidades (ex: suporte a novos sensores, filtros de UI)
* `fix:` Correcoes de bugs ou excecoes (ex: tratamento de excecoes de drivers, politicas SAC)
* `refactor:` Reorganizacao e modularizacao de codigo sem alteracao de comportamento externo
* `docs:` Adicao ou revisao de documentacao técnica

Exemplo:
```text
refactor(ui): decompor MainForm em classes parciais e criar hub docs

- Decompor MainForm.cs em MainForm.Tray.cs, MainForm.WebServer.cs, MainForm.Plot.cs e MainForm.Tree.cs
- Criar documentacao estruturada em docs/ (INDEX.md, FILE_TREE.md, RELATORIO, AGENTS)
- Assegurar compilacao com 0 avisos e 0 erros em .NET 8.0
```
