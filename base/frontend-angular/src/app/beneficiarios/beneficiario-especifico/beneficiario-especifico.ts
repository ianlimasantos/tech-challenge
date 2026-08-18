import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Beneficiario } from '../beneficiario';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { Loading } from '../../nucleo/loading/loading';
import { ModalExcluir } from '../../nucleo/modal-excluir/modal-excluir';
import { ModalMensagem } from '../../nucleo/modal-mensagem/modal-mensagem';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PlanoServico } from '../../planos/plano-servico';
import { Plano } from '../../planos/plano';
import { BeneficiariosServico } from '../beneficiario-servico';
import { HttpErrorResponse } from '@angular/common/http';
import { mensagemDeErro } from '../../nucleo/api';

@Component({
  selector: 'app-beneficiario-especifico',
  imports: [FormsModule, RouterLink, ModalMensagem, ModalExcluir, Loading],
  templateUrl: './beneficiario-especifico.html',
  styleUrl: './beneficiario-especifico.css',
})
export class BeneficiarioEspecifico {

  protected readonly modalExcluir = signal(false);
  protected beneficiarioIdExcluir: string | null = null;
  protected erro = signal<string | null>(null);
  readonly titulo = "Excluir beneficiário";
  readonly mensagem = "Tem certeza que deseja excluir este beneficiário?";
  protected beneficiarioIdBusca = '';
  protected beneficiarioBusca = signal<Beneficiario | null>(null);
  protected carregando = signal(false);
  private readonly planoServico = inject(PlanoServico);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly planos = signal<Plano[]>([]);
  private readonly router = inject(Router);
  protected beneficiarioServico = inject(BeneficiariosServico);

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

  protected buscarPorId() {

    if (!this.beneficiarioIdBusca.trim()) {
      return;
    }

    this.erro.set(null);
    this.beneficiarioBusca.set(null);
    this.carregando.set(true);

    this.beneficiarioServico
      .obter(this.beneficiarioIdBusca.trim())
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.carregando.set(false))
      )
      .subscribe({
        next: (resposta) => {
          this.beneficiarioBusca.set(resposta);
        },

        error: (resposta: HttpErrorResponse) => {
          this.erro.set(mensagemDeErro(resposta));
        }
      });
  }

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
          this.buscarPorId()
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
