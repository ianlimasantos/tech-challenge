import { Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Plano } from '../../planos/plano';
import { BeneficiarioRequest } from '../beneficiario';
import { BeneficiariosServico } from '../beneficiario-servico';
import { PlanoServico } from '../../planos/plano-servico';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { mensagemDeErro } from '../../nucleo/api';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-beneficiario-form',
  imports: [ReactiveFormsModule],
  templateUrl: './beneficiario-form.html',
  styleUrl: './beneficiario-form.css',
})
export class BeneficiarioForm {

  private readonly beneficiarioServico = inject(BeneficiariosServico);
  private readonly planoServico = inject(PlanoServico);
  private fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly planos = signal<Plano[]>([]);
  protected readonly erro = signal<string | null>(null);
  protected readonly carregando = signal(true);
  private readonly route = inject(ActivatedRoute);
  protected readonly editando = signal(false);
  private readonly router = inject(Router);
  id: string = '';

  ngOnInit(){

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


  protected salvar(){
    const valor = this.formulario.getRawValue();
    const dados: BeneficiarioRequest = {
      ...valor
    }


    if(!this.id){
      this.beneficiarioServico
        .cadastrar(dados)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            console.log("salvo");
            this.carregando.set(false);
          },
          error: (resposta: HttpErrorResponse) => {
            this.erro.set(mensagemDeErro(resposta))
            this.carregando.set(false);
          }
        })
    }else{
      this.beneficiarioServico
        .atualizar(this.id, dados)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            console.log("salvo");
            this.carregando.set(false);
          },
          error: (resposta: HttpErrorResponse) => {
            this.erro.set(mensagemDeErro(resposta))
            this.carregando.set(false);
          }
        })
    }
  }

  private carregarBeneficiario(): void {
    this.beneficiarioServico.obter(this.id).subscribe({
      next: (beneficiario) => {
        this.formulario.patchValue({
          nome_completo: beneficiario.nome_completo,
          cpf: beneficiario.cpf,
          data_nascimento: beneficiario.data_nascimento,
          plano_id: beneficiario.plano_id,
          status: beneficiario.status
        });
      }
    });
  }

  protected cancelar(): void {
    this.router.navigate(['/beneficiario-lista']);
  }
}
