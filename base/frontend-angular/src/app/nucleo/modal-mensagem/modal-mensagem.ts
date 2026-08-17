import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-modal-mensagem',
  imports: [],
  templateUrl: './modal-mensagem.html',
  styleUrl: './modal-mensagem.css',
})
export class ModalMensagem {

  mensagem = input.required<string>();
  fechar = output<void>();

}
