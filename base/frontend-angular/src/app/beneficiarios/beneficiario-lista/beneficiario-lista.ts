import { Component, DestroyRef, inject, signal } from '@angular/core';
import { BeneficiariosServico } from '../beneficiario-servico';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Beneficiario, PaginaResponse } from '../beneficiario';
import { PlanoServico } from '../../planos/plano-servico';
import { Plano } from '../../planos/plano';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Router } from '@angular/router';

@Component({
  selector: 'app-beneficiario-lista',
  imports: [FormsModule, RouterLink],
  templateUrl: './beneficiario-lista.html',
  styleUrl: './beneficiario-lista.css',
})
export class BeneficiarioLista {

  protected readonly beneficiarios = signal<Beneficiario[]>([]);
  protected beneficiarioServico = inject(BeneficiariosServico);
  private readonly planoServico = inject(PlanoServico);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly planos = signal<Plano[]>([]);
  protected readonly carregando = signal(false);
  private readonly router = inject(Router);
  protected pagina = 1;
  protected tamanho = 10;
  protected total = 0;
  status?: 'ATIVO' | 'INATIVO' | '' = '';
  planoId?: string = '';
  protected readonly modalExcluir = signal(false);
  protected beneficiarioIdExcluir: string | null = null;


  ngOnInit(){
    this.planoServico.listar()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (planos)=> {
          this.planos.set(planos);
          console.log("okay");
        }
      })
  }


  protected pesquisar(){
    this.carregando.set(true);
    this.beneficiarioServico.listar(this.pagina, this.tamanho, this.status, this.planoId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (resposta) => {
          this.beneficiarios.set(resposta.dados);
          this.total = resposta.total;
          this.carregando.set(false);
        },
        error: () => {
          this.carregando.set(false);
        }
      })
  }

  protected limparFiltros(){
    this.status = '';
    this.planoId = '';
  }

  protected paginaAnterior(){}

  protected proximaPagina(){}

  protected editar(id: string){
    this.router.navigate(['/beneficiarios', id, 'editar']);
  }

  protected excluir(id: string){
    this.beneficiarioIdExcluir = id;
    this.modalExcluir.set(true);
  }

  protected cancelarExclusao(): void{
    this.modalExcluir.set(false);
    this.beneficiarioIdExcluir = null;
  }

  protected confirmarExclusao(){
    if(!this.beneficiarioIdExcluir){
      return;
    }

    this.beneficiarioServico.excluir(this.beneficiarioIdExcluir)
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: () => {
        this.modalExcluir.set(false);
        this.beneficiarioIdExcluir = null;
      },
      error: (erro) => {
        console.error(erro);
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
