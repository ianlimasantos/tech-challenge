# 1. Resumo da entrega

Implementei os endpoints faltantes de Beneficiários e corrigi os existentes, usando o módulo de Planos como referência de estrutura. Separei as responsabilidades entre as camadas, retirando o acesso direto ao banco da controller e utilizando requests, responses e serviços. Adaptei as validações e o tratamento de erros ao contrato da especificação. Também implementei paginação, filtros e ordenação na listagem.

No frontend, implementei a interface de Beneficiários com cadastro, edição, exclusão, filtros, paginação, modais para feedback das operações e loading, mantendo o acesso à API isolado nos serviços. Também corrigi um defeito existente no recarregamento de Planos e adicionei testes para os principais comportamentos. Tive auxílio de IA principalmente na parte de testes, por ser uma área que ainda estou desenvolvendo.

---

# 2. Decisões

## 2.1 Defeitos que encontrei no código base

### 1. Camadas misturadas na controller

* **Onde:** `\tech-challenge\base\backend-dotnet\src\Desafio.Api\Controllers\BeneficiariosController.cs`
* **O que estava errado:** A controller acessava diretamente o banco e também continha lógica que deveria pertencer a outras camadas.
* **Como percebi:** Não havia nenhum serviço no `BeneficiariosController` e havia uma criação explícita do acesso ao banco de dados.
* **Como corrigi:** Coloquei o `BeneficiarioServico` entre a controller e o `AppDbContext`.
* **O que quebraria em produção:** A aplicação continuaria funcionando, mas a controller ficaria excessivamente acoplada ao acesso a dados e à lógica de negócio. Com o crescimento da aplicação, isso dificultaria manutenção, testes e reutilização da lógica.

### 2. Beneficiário mapeado incorretamente no banco

* **Onde:** `tech-challenge\base\backend-dotnet\src\Desafio.Api\Infraestrutura\AppDbContext.cs`
* **O que estava errado:** Faltava a configuração da data de cadastro e de nascimento como campos obrigatórios.
* **Como percebi:** Fiz uma conferência geral da entidade quando fui adicionar a unicidade do CPF.
* **Como corrigi:** Adicionei:

```csharp
entidade.Property(b => b.DataNascimento).IsRequired();
entidade.Property(b => b.DataCadastro).IsRequired();
```

* **O que quebraria em produção:** O modelo do banco não estaria refletindo corretamente a obrigatoriedade definida para esses campos. Isso poderia permitir inconsistências entre o contrato da aplicação e a persistência dos dados, resultando em valores nulos onde a aplicação esperava dados obrigatórios.

### 3. Recarregar planos não funcionava no frontend

* **Onde:** `tech-challenge/base/frontend-angular/src/app/planos/planos-lista.ts`
* **O que estava errado:** `takeUntilDestroyed()` estava sendo utilizado fora de um contexto de injeção válido.
* **Como percebi:** Vi que o botão de carregar planos não funcionava no frontend e que aparecia um erro no console.
* **Como corrigi:** Como não sabia como o `takeUntilDestroyed()` funcionava, pedi à IA para me explicar o problema. A partir disso, utilizei um `DestroyRef` no componente e passei essa referência explicitamente para `takeUntilDestroyed(this.destroyRef)`.
* **O que quebraria em produção:** O usuário não conseguiria recarregar a listagem de Planos pelo botão e precisaria recarregar a página inteira para obter os dados novamente.

---

## 2.2 Pontos em que a especificação não definiu o comportamento

### 1. Ordenação da paginação

* **O que a spec não define:** A ordenação da paginação.
* **O que decidi:** Ordenar por `NomeCompleto` e, em seguida, por `Id`.
* **Por quê:** O segundo critério funciona como desempate e torna a ordem determinística quando dois beneficiários possuem o mesmo nome.
* **O que eu consideraria se fosse decidir diferente:** Poderia utilizar a data de cadastro como segundo critério.

### 2. Busca de beneficiário por ID no frontend

* **O que a spec não define:** A interface não especifica uma tela ou fluxo separado para consultar um beneficiário pelo ID.
* **O que decidi:** Mantive o endpoint `GET /beneficiarios/{id}` implementado na API e disponibilizei acesso a ele na interface.
* **Por quê:** O endpoint é exigido pelo contrato da API e possui comportamentos específicos para sucesso e recurso não encontrado. Por isso, mantive o endpoint e disponibilizei uma forma de utilizá-lo na interface.
* **O que eu consideraria se fosse decidir diferente:** Poderia tratar a busca por ID como mais um filtro da listagem, pois criar um componente separado para isso fez com que parte da lógica do componente de listagem precisasse ser repetida.

---

## 2.3 Inconsistências que percebi

### 1. Tamanho padrão da página

* **A spec diz:** Quando o tamanho não for informado, devem ser utilizados 10 itens.
* **O teste ou código existente esperava:** O teste esperava 20.
* **Segui:** A especificação, mantendo o padrão em 10.
* **Por quê:** A especificação define explicitamente 10 como valor padrão. Mesmo havendo uma divergência no teste existente, priorizei o contrato da API. Além disso, é o padrão que, em geral, eu noto nas aplicações reais e não acho que faz sentido desviarmos desse padrão.

### 2. Mistura de responsabilidades

* **A spec diz:** A regra de negócio não vive na controller e o domínio deve ser isolado de framework e de acesso a dados.
* **O teste ou código existente esperava:** Services e isolamento de responsabilidades.
* **Segui:** A especificação. Fiz a alteração do código-fonte, criei um `BeneficiarioService` e removi a lógica e o acesso ao banco de dados da controller.
* **Por quê:** A controller estava sobrecarregada com responsabilidades que não eram dela, aumentando o acoplamento entre a camada HTTP e o acesso a dados.

---

## 2.4 Decisões técnicas

1. Utilizei `IQueryable` no serviço de listagem para que filtros, ordenação e paginação fossem executados pelo banco, em vez de carregar todos os registros para a memória.

2. Utilizei `AsNoTracking()` na consulta de listagem, pois os registros são apenas lidos e não precisam ser rastreados pelo Entity Framework.

3. No frontend, utilizei Angular Signals para estados como carregamento, erro, beneficiários e estado dos modais.

4. Utilizei `finalize()` nas chamadas HTTP para garantir que o loading seja encerrado tanto em caso de sucesso quanto de erro.

5. Como são poucos planos, não alterei o contrato de Beneficiários para retornar o nome do plano. Fiz a correspondência no frontend.

6. Sobre o tamanho da página no frontend, coloquei opções fixas (`10`, `20` e `50`) porque é uma abordagem comum em aplicações reais. Mantive, porém, as validações no backend, então a API continua validando tamanhos entre 1 e 100 caso seja consumida diretamente.

7. Utilizei `[FromQuery(Name = "plano_id")]` no parâmetro correspondente para garantir o mapeamento explícito do `snake_case` utilizado pela API para o nome da propriedade em C#. Isso evitou problemas de binding dos parâmetros vindos da URL.

8. Na validação do POST de cadastro do beneficiário, coloquei a regra de negócio de bloquear uma data de nascimento futura e retornar `422`, conforme definido na especificação. Essa resposta ocorre quando esse é o único problema encontrado. Caso existam outros erros de validação, a API retorna `400 Bad Request`.

---

## 2.5 O que ficou de fora

Não implementei funcionalidades que não eram necessárias para o contrato principal apresentado pela especificação. Acredito ter atendido às especificações, além de passar em todos os testes e obter aprovação total ao executar o `verificar.sh`.

---

# 3. Uso de IA

**Nível de uso:** intenso. No código, já declarei ao longo das seções o uso da IA. Mas gostaria também de avisar que, na escrita desse documento, também utilizei IA. Primeiro, respondi todas as questões e depois solicitei uma revisão da escrita. 

## 3.1 Ferramentas

* **ChatGPT:** utilizado para esclarecer conceitos, analisar erros, discutir alternativas de implementação, revisar código e auxiliar na elaboração de testes.
* **Docker Desktop:** utilizado para executar a API, PostgreSQL e ambiente frontend em containers.
* **Git Bash:** utilizado para executar o script `verificar.sh` do desafio e gerar as imagens no Docker Hub.

## 3.2 Os 3 prompts que mais influenciaram o resultado

### Prompt 1

```text
Analise a estrutura de testes existente e me ajude a criar testes que realmente validem o comportamento da API. Qro que seja testes para cadastro, consulta, atualização, exclusão, filtros e paginação
(eu havia mandando antes alguns testes que já existiam para dar um contexto)
```

* **O que aceitei:** A maior parte da implementação dos testes gerada pela IA, utilizando a estrutura de testes que já existia no projeto e cobrindo os comportamentos que eu havia solicitado. Depois, pedi explicações sobre os testes e validei se os comportamentos testados estavam de acordo com a especificação.
* **O que descartei e por quê:** Descartei testes ou comportamentos que não estivessem relacionados ao contrato da API ou aos requisitos do desafio.

### Prompt 2

```text
Tenho um botão que não tá funcionando e no console tem um erro falando do takeUntilDestroyed(), me explica o que pode estar causando isso
```

* **O que aceitei:** A explicação de que o `takeUntilDestroyed()` estava sendo utilizado fora de um contexto de injeção válido. A partir disso, utilizei um `DestroyRef` no componente e passei essa referência explicitamente para `takeUntilDestroyed(this.destroyRef)`.
* **O que descartei e por quê:** Não houve uma sugestão relevante que eu precisasse descartar nesse caso. A explicação ajudou a identificar a causa do erro e a correção proposta fazia sentido para o problema. Inclusive, depois de entender essa abordagem, passei a utilizá-la em outros componentes quando precisava do mesmo comportamento.

### Prompt 3

```text
Cria para mim o layout desse componente de cadastro de beneficiario baseado nesse form
```

* **O que aceitei:** Aproveitei o CSS gerado pela IA como ponto de partida para o layout do formulário.
* **O que descartei e por quê:** Descartei partes que utilizavam componentes, efeitos visuais e estruturas de SCSS com os quais não estava habituado e que teria dificuldade para explicar ou manter. Preferi manter uma solução mais simples, utilizando principalmente o CSS que conseguia compreender e modificar.

## 3.3 O que fiz sem IA

As decisões de arquitetura, organização e comportamento da aplicação partiram de mim. Eu procurava entender qual solução considerava adequada para o problema. A partir da análise da arquitetura existente e da especificação, entendi que o acesso ao banco de dados deveria ficar no service e que a lógica de Beneficiários deveria permanecer nessa camada. Isso também veio da análise do módulo existente, pois geralmente trabalho com classes anêmicas que refletem somente os atributos do banco de dados.

Um exemplo de organização foi a construção do domínio. A implementação dos métodos `Atualizar` e `DefinirDados` no domínio de Beneficiário ficou inicialmente repetida entre os métodos. Ao perceber isso, decidi concentrar as validações comuns em um único método e manter em cada operação apenas as regras específicas daquele comportamento.

No frontend, as decisões sobre quais interações seriam necessárias, quais estados deveriam existir e como o usuário deveria ser informado partiram de mim. Por exemplo, defini a utilização de modais para cadastro, edição e exclusão e um estado de loading durante as requisições.

Portanto, embora a IA tenha acelerado bastante a escrita e a resolução de problemas, continuei responsável por definir a estrutura, avaliar as alternativas e decidir o que seria incorporado ao projeto.

## 3.4 O que ainda não domino

Ainda tenho dificuldade para compreender toda a estrutura de testes existente e não tenho experiência prática significativa com testes automatizados. Por isso, precisei de bastante auxílio da IA nessa parte.

Após a criação dos testes, pedia que a IA explicasse como eles funcionavam e validava se os comportamentos estavam de acordo com a especificação. Inclusive, foi por isso que, na próxima seção, coloquei um dos testes gerados como o trecho mais complexo.

Agora que identifiquei essa deficiência, pretendo estudar mais sobre testes automatizados para me preparar melhor para a entrevista.

---

# 4. Perguntas de compreensão

## 4.1 Concorrência

**O que acontece se duas requisições simultâneas tentarem criar beneficiários com o mesmo CPF? Onde exatamente, na sua implementação, a unicidade é garantida?**

Duas requisições podem tentar cadastrar o mesmo CPF praticamente ao mesmo tempo. Uma verificação prévia para consultar o CPF na base não seria suficiente, porque as duas requisições poderiam consultar o banco antes que qualquer uma delas fosse persistida.

Por isso, a garantia definitiva de unicidade está no banco de dados, por meio de uma restrição única sobre o CPF. Eu coloquei, no arquivo `AppDbContext`, na definição do Beneficiário, o seguinte código:

```csharp
entidade.HasIndex(b => b.Cpf).IsUnique();
```

Agora, se as duas requisições tentarem inserir o mesmo CPF, apenas uma consegue persistir. A outra recebe uma violação de unicidade, identificada pelo código `23505` do PostgreSQL, que deve ser convertida para o `409 Conflict` esperado pela API.

---

## 4.2 Um defeito que você corrigiu

**Escolha um dos defeitos que encontrou no código base e explique: por que o código original estava errado, e em que situação real ele quebraria em produção?**

Um dos defeitos que corrigi estava no módulo de Planos, no botão de recarregar a listagem. Ao clicar no botão, a tela não carregava novamente os planos, embora a chamada ao serviço estivesse implementada.

Investigando o problema, percebi que o método responsável por recarregar os dados utilizava `takeUntilDestroyed()` fora de um contexto de injeção válido. Esse operador precisa receber um `DestroyRef` injetado no componente ou ser chamado em um contexto onde o Angular consiga resolver a instância atual. Caso contrário, a execução da chamada era interrompida, gerando um erro no console, e a listagem não era atualizada.

Corrigi o componente injetando o `DestroyRef` e passando essa referência explicitamente para `takeUntilDestroyed(this.destroyRef)`. Com isso, a inscrição passou a ser criada corretamente e o botão de recarregar voltou a buscar os planos normalmente.

Em produção, esse defeito impediria o usuário de atualizar a listagem de Planos pelo botão, fazendo com que ele precisasse recarregar a página inteira para obter os dados novamente.

---

## 4.3 O trecho mais complexo

Escolhi o teste `Percorrer_todas_as_paginas_deve_devolver_cada_registro_exatamente_uma_vez`, pois ele valida uma regra importante da especificação: ao percorrer todas as páginas, nenhum beneficiário pode ser perdido ou aparecer mais de uma vez.

```csharp
[Fact]
public async Task Percorrer_todas_as_paginas_deve_devolver_cada_registro_exatamente_uma_vez()
{
    var semeados = await fixture.SemearBeneficiariosAsync(23);
    const int tamanho = 7;

    var idsColetados = new List<Guid>();
    var pagina = 1;

    while (true)
    {
        var corpo = await (await Client.GetAsync($"/beneficiarios?pagina={pagina}&tamanho={tamanho}")).CorpoAsync();
        var itens = corpo.GetProperty("dados").EnumerateArray().ToList();

        if (itens.Count == 0)
        {
            break;
        }

        idsColetados.AddRange(itens.Select(item => item.GetProperty("id").GetGuid()));
        pagina++;
    }

    Assert.Equal(semeados.Count, idsColetados.Count);
    Assert.Equal(semeados.Count, idsColetados.Distinct().Count());
    Assert.Equal(semeados.Select(s => s.Id).OrderBy(id => id), idsColetados.OrderBy(id => id));
}
```

**`[Fact]`** — Indica que o método é um teste do xUnit.

**`public async Task`** — Define o método do teste. Ele é assíncrono porque realiza operações de banco de dados e requisições HTTP que precisam ser aguardadas.

**`var semeados = await fixture.SemearBeneficiariosAsync(23);`** — Cria 23 beneficiários no banco de dados. O resultado é armazenado em `semeados` para que os IDs dos registros criados possam ser comparados posteriormente com os IDs retornados pela API.

**`const int tamanho = 7;`** — Define que cada página terá no máximo 7 registros.

**`var idsColetados = new List<Guid>();`** — Cria uma lista vazia que será utilizada para armazenar os IDs dos beneficiários encontrados durante a navegação pelas páginas.

**`var pagina = 1;`** — Define que a primeira página consultada será a página 1.

**`while (true)`** — Inicia um loop que continuará executando até que seja encontrada uma página sem registros.

**`var corpo = await (await Client.GetAsync($"/beneficiarios?pagina={pagina}&tamanho={tamanho}")).CorpoAsync();`** — Faz uma requisição GET para o endpoint de Beneficiários, informando a página atual e o tamanho da página. O primeiro `await` aguarda a resposta HTTP e o segundo aguarda a leitura do corpo da resposta.

**`var itens = corpo.GetProperty("dados").EnumerateArray().ToList();`** — Acessa a propriedade `dados` do JSON retornado pela API, percorre os elementos desse array e transforma o resultado em uma lista.

**`if (itens.Count == 0)`** — Verifica se a página retornou algum registro.

**`break`** — Se a página estiver vazia, significa que não existem mais registros para percorrer. Nesse caso, o loop é encerrado.

**`idsColetados.AddRange(itens.Select(item => item.GetProperty("id").GetGuid()));`** — Percorre os itens da página, obtém o ID de cada beneficiário, converte o valor para `Guid` e adiciona todos os IDs à lista `idsColetados`.

**`pagina++`** — Incrementa o número da página para que a próxima requisição consulte a página seguinte.

**`Assert.Equal(semeados.Count, idsColetados.Count);`** — Verifica se a quantidade de registros coletados é igual à quantidade de registros criados inicialmente. Isso ajuda a identificar se algum registro foi perdido ou se foram retornados registros adicionais.

**`Assert.Equal(semeados.Count, idsColetados.Distinct().Count());`** — Compara a quantidade total de IDs coletados com a quantidade de IDs distintos. Se os valores forem iguais, não existem duplicidades na coleção coletada.

**`Assert.Equal(semeados.Select(s => s.Id).OrderBy(id => id), idsColetados.OrderBy(id => id));`** — Compara os IDs dos beneficiários criados inicialmente com os IDs coletados durante a paginação. O `OrderBy` ordena as duas coleções antes da comparação, fazendo com que a ordem dos registros não influencie o resultado.
