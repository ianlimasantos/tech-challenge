import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-modal-excluir',
  imports: [],
  templateUrl: './modal-excluir.html',
  styleUrl: './modal-excluir.css',
})
export class ModalExcluir {

  titulo = input.required<string>();
  mensagem = input.required<string>();
  cancelar = output<void>();
  excluir = output<void>();

}
