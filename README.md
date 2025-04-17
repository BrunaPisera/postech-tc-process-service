<div align="center">
<img src="https://github.com/user-attachments/assets/208a0ebb-ca7c-4b0b-9f68-0b35050a9880" width="30%" />
</div>

# VidProc- Video Processing Service (POS TECH: TECH CHALLENGE - FASE FINAL)🚀

Seja bem vindo! Este é um desafio proposto pela PósTech (Fiap + Alura) na ultima fase da pós graduação de Software Architecture (8SOAT).

📼 Vídeo de demonstração do projeto desta fase: em produção

Integrantes do grupo:<br>
Alexis Cesar (RM 356558)<br>
Bruna Gonçalves (RM 356557)

O Video Processing Service é responsável por consumir as mensagens da fila RabbitMQ, pegar o vídeo armazenado no Amazon S3, processá-lo (gerando imagens) e armazenar essas imagens no S3. Após o processamento, o vídeo original é deletado.

A aplicação é containerizada utilizando Docker, orquestrada por Kubernetes (K8s) para garantir escalabilidade e resiliência, e gerenciada por Helm, que automatiza o deployment e rollbacks no cluster Kubernetes (EKS) na nuvem da AWS.

## Navegação
- [Fluxo](#fluxo)
- [Tecnologias Utilizadas](#tecnologias-utilizadas)

## Fluxo
 
1. O serviço consome a mensagem do RabbitMQ que indica que um vídeo foi enviado.
2. O vídeo é baixado do S3.
3. O vídeo é processado usando FFmpeg para gerar imagens (frames).
4. As imagens são armazenadas no S3.
5. O vídeo original é apagado do S3.

O serviço envia uma nova mensagem para informar o status do processamento e fornecer o link das imagens geradas.
## Tecnologias Utilizadas
 
- **API Gateway**: Exposição da API.
- **Amazon S3**: Armazenamento de vídeos.
- **RabbitMQ (via CloudAMQP)**: Fila de mensagens para notificar o processamento.
- **Kubernetes**: Orquestração de contêineres.
- **Helm**: Gerenciamento de de pacotes kubernetes.
- **EKS**: Cluster Kubernetes na nuvem da AWS.
- **Terraform**: Automação de criação de recursos em provedores de nuvem.

ℹ️ Este repositório faz parte de um conjunto de repositórios (outros serviços, infraestrutura e banco de dados) que formam o sistema VidProc. Link de todos os repositórios envolvidos:
- [Infraestrutura AWS](https://github.com/BrunaPisera/postech-tc-infra)
- [API de StatusTracking](https://github.com/BrunaPisera/postech-tc-status-tracking-api)
- [API de upload](https://github.com/BrunaPisera/postech-tc-upload)
- [Serviço de Processamento](https://github.com/BrunaPisera/postech-tc-process-service)
- [Banco de Dados](https://github.com/BrunaPisera/postech-tc-db)
