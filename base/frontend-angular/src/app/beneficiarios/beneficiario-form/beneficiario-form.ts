import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs';
import { mensagemDeErro } from '../../nucleo/api';
import { Loading } from '../../nucleo/loading/loading';
import { Plano } from '../../planos/plano';
import { PlanoServico } from '../../planos/plano-servico';
import { BeneficiarioRequest } from '../beneficiario';
import { BeneficiariosServico } from '../beneficiario-servico';
import { ModalMensagem } from '../../nucleo/modal-mensagem/modal-mensagem';

@Component({
  selector: 'app-beneficiario-form',
  imports: [ReactiveFormsModule, ModalMensagem, Loading],
  templateUrl: './beneficiario-form.html',
  styleUrl: './beneficiario-form.css',
})
export class BeneficiarioForm {

  private readonly beneficiarioServico = inject(BeneficiariosServico);
  private readonly planoServico = inject(PlanoServico);
  private fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly planos = signal<Plano[]>([]);
  protected erro = signal<string | null>(null);
  protected carregando = signal(false);
  private readonly route = inject(ActivatedRoute);
  protected readonly editando = signal(false);
  private readonly router = inject(Router);
  readonly mensagem = "O beneficiário foi cadastrado/atualizado com sucesso no sistema!";
  protected cadastroEfetivado = signal(false);

  id: string = '';

  ngOnInit() {

    const id = this.route.snapshot.paramMap.get('id');

    if (id) {
      this.editando.set(true);
      this.id = id;
      this.carregarBeneficiario();
    }
    this.planoServico.listar()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (planos) => {
          this.planos.set(planos);
        },
        error: (resposta: HttpErrorResponse) => {
          this.erro.set(mensagemDeErro(resposta));
          this.carregando.set(false);
        }
      })
  }

  formulario = this.fb.group({
    nome_completo: [
      '',
      [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(120)
      ]
    ],

    cpf: [
      '',
      [
        Validators.required,
        Validators.pattern(/^\d{11}$/)
      ]
    ],

    data_nascimento: [
      '',
      Validators.required
    ],

    plano_id: [
      '',
      Validators.required
    ],

    status: ['ATIVO' as 'ATIVO' | 'INATIVO']
  });


  protected salvar() {

    this.carregando.set(true);
    const valor = this.formulario.getRawValue();
    const dados: BeneficiarioRequest = {
      ...valor
    }


    if (!this.id) {
      this.beneficiarioServico
        .cadastrar(dados)
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          finalize(() => { this.carregando.set(false) })
        )
        .subscribe({
          next: () => {
            this.cadastroEfetivado.set(true);
          },
          error: (resposta: HttpErrorResponse) => {
            this.erro.set(mensagemDeErro(resposta));
          }
        })
    } else {
      this.beneficiarioServico
        .atualizar(this.id, dados)
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          finalize(() => { this.carregando.set(false) })
        )
        .subscribe({
          next: () => {
            console.log("salvo");
            this.carregando.set(false);
            this.cadastroEfetivado.set(true);
          },
          error: (resposta: HttpErrorResponse) => {
            this.erro.set(mensagemDeErro(resposta))
            this.carregando.set(false);
          }
        })
    }
  }

  private carregarBeneficiario(): void {
    this.carregando.set(true);
    this.beneficiarioServico.obter(this.id)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => { this.carregando.set(false) })
      )
      .subscribe({
        next: (beneficiario) => {
          this.formulario.patchValue({
            nome_completo: beneficiario.nome_completo,
            cpf: beneficiario.cpf,
            data_nascimento: beneficiario.data_nascimento,
            plano_id: beneficiario.plano_id,
            status: beneficiario.status
          });
        },
        error: (resposta: HttpErrorResponse) => {
          this.erro.set(mensagemDeErro(resposta));
        }
      });
  }

  protected redirecionarLista(): void {
    this.router.navigate(['/beneficiario-lista']);
  }


}
