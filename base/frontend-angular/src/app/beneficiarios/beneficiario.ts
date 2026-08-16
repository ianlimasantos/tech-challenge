export interface Beneficiario{
  id: string;
  nome_completo: string;
  cpf: string;
  data_nascimento: string;
  status: 'ATIVO' | 'INATIVO';
  plano_id: string;
  data_cadastro: string;
}


export interface BeneficiarioRequest{
  nome_completo: string | null;
  cpf?: string | null;
  data_nascimento: string | null;
  plano_id: string | null;
  status?: 'ATIVO' | 'INATIVO' | null;
}


export interface PaginaResponse<T> {
  dados: T;
  pagina: number;
  tamanho: number;
  total: number;
}
