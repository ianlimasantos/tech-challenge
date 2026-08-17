import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { mensagemDeErro } from '../../nucleo/api';
import { Loading } from '../../nucleo/loading/loading';
import { ModalExcluir } from '../../nucleo/modal-excluir/modal-excluir';
import { Plano } from '../../planos/plano';
import { PlanoServico } from '../../planos/plano-servico';
import { Beneficiario } from '../beneficiario';
import { BeneficiariosServico } from '../beneficiario-servico';
import { ModalMensagem } from '../../nucleo/modal-mensagem/modal-mensagem';

@Component({
  selector: 'app-beneficiario-lista',
  imports: [FormsModule, RouterLink, ModalMensagem, ModalExcluir, Loading],
  templateUrl: './beneficiario-lista.html',
  styleUrl: './beneficiario-lista.css',
})
export class BeneficiarioLista {

  protected readonly beneficiarios = signal<Beneficiario[]>([]);
  protected beneficiarioServico = inject(BeneficiariosServico);
  private readonly planoServico = inject(PlanoServico);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly planos = signal<Plano[]>([]);
  protected carregando = signal(false);
  private readonly router = inject(Router);
  protected pagina = 1;
  protected tamanho = 10;
  protected total = 0;
  status?: 'ATIVO' | 'INATIVO' | '' = '';
  planoId?: string = '';
  protected readonly modalExcluir = signal(false);
  protected beneficiarioIdExcluir: string | null = null;
  protected erro = signal<string | null>(null);
  readonly titulo = "Excluir beneficiário";
  readonly mensagem = "Tem certeza que deseja excluir este beneficiário?";



  ngOnInit() {
    this.carregando.set(true);
    this.planoServico.listar()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => { this.carregando.set(false) })
      )
      .subscribe({
        next: (planos) => {
          this.planos.set(planos);
        },
        error: (resposta: HttpErrorResponse) => {
          this.erro.set(mensagemDeErro(resposta))
        }
      })
  }


  protected pesquisar() {
    this.carregando.set(true);
    this.beneficiarioServico.listar(this.pagina, this.tamanho, this.status, this.planoId)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => { this.carregando.set(false) })
      )
      .subscribe({
        next: (resposta) => {
          this.beneficiarios.set(resposta.dados);
          this.total = resposta.total;
        },
        error: (resposta: HttpErrorResponse) => {
          this.erro.set(mensagemDeErro(resposta))
        }
      })
  }

  protected limparFiltros() {
    this.status = '';
    this.planoId = '';
  }

  protected paginaAnterior() { }

  protected proximaPagina() { }

  protected editar(id: string) {
    this.router.navigate(['/beneficiarios', id, 'editar']);
  }

  protected excluir(id: string) {
    this.beneficiarioIdExcluir = id;
    this.modalExcluir.set(true);
  }

  protected cancelarExclusao(): void {
    this.modalExcluir.set(false);
    this.beneficiarioIdExcluir = null;
  }

  protected confirmarExclusao() {
    if (!this.beneficiarioIdExcluir) {
      return;
    }

    this.carregando.set(true);
    this.beneficiarioServico.excluir(this.beneficiarioIdExcluir)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => { this.carregando.set(false) })
      )
      .subscribe({
        next: () => {
          this.modalExcluir.set(false);
          this.pesquisar()
          this.beneficiarioIdExcluir = null;
        },
        error: (resposta: HttpErrorResponse) => {
          this.erro.set(mensagemDeErro(resposta))
        }
      });
  }

  protected nomePlano(planoId: string): string {
    const plano = this.planos().find(
      plano => plano.id === planoId
    );

    return plano?.nome ?? 'Plano não encontrado';
  }
}
