import { inject, Injectable } from '@angular/core';
import { Beneficiario, BeneficiarioRequest, PaginaResponse } from './beneficiario';
import { Observable } from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';
import { API_BASE } from '../nucleo/api';

@Injectable({
  providedIn: 'root',
})
export class BeneficiariosServico {
  private readonly http = inject(HttpClient);
  private readonly base = inject(API_BASE);



  cadastrar(request: BeneficiarioRequest ): Observable<Beneficiario>{
    return this.http.post<Beneficiario>(`${this.base}/beneficiarios`, request);
  }

  atualizar(id: string, request: BeneficiarioRequest ): Observable<Beneficiario>{
    return this.http.put<Beneficiario>(`${this.base}/beneficiarios/${id}`, request);
  }

  obter(id: string):Observable<Beneficiario> {
    return this.http.get<Beneficiario>(`${this.base}/beneficiarios/${id}`);
  }

  listar(pagina: number = 1, tamanho: number = 10, status?: 'ATIVO' | 'INATIVO' | '', planoId?: string): Observable<PaginaResponse<Beneficiario[]>>{

    let params = new HttpParams()
    .set('pagina', pagina)
    .set('tamanho', tamanho);

    if (status) {
      params = params.set('status', status);
    }

    if (planoId) {
      params = params.set('planoid', planoId);
    }

    return this.http.get<PaginaResponse<Beneficiario[]>>(`${this.base}/beneficiarios`, { params });
  }

  excluir(id: string): Observable<void> {
    return this.http.delete<void>(
      `${this.base}/beneficiarios/${id}`
    );
  }
}


