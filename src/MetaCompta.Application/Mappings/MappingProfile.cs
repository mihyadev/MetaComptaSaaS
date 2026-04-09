using AutoMapper;
using MetaCompta.Application.DTOs;
using MetaCompta.Domain.Common;
using MetaCompta.Domain.Entities;

namespace MetaCompta.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Client
        CreateMap<Client, ClientDto>()
            .ForMember(d => d.ManagerPrincipalName, opt => opt.MapFrom(s => s.ManagerPrincipal != null ? s.ManagerPrincipal.FullName : null))
            .ForMember(d => d.EmployeeCount, opt => opt.MapFrom(s => s.Employees.Count))
            .ForMember(d => d.DeclarationCount, opt => opt.MapFrom(s => s.Declarations.Count))
            .ForMember(d => d.ServicesFiduciaires, opt => opt.MapFrom(s => s.ServiceContrats));
        CreateMap<ClientCreateDto, Client>()
            .ForMember(d => d.ServiceContrats, opt => opt.Ignore()); // handled manually

        // ServiceContrat
        CreateMap<ServiceContrat, ServiceContratDto>();
        CreateMap<ServiceContratCreateDto, ServiceContrat>()
            .ForMember(d => d.DateDebut, opt => opt.MapFrom(s => s.DateDebut ?? DateTime.UtcNow));

        // Employee
        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.RaisonSociale : null));
        CreateMap<EmployeeCreateDto, Employee>();

        // Declaration
        CreateMap<Declaration, DeclarationDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.RaisonSociale : null))
            .ForMember(d => d.AssistantName, opt => opt.MapFrom(s => s.Assistant != null ? s.Assistant.FullName : null))
            .ForMember(d => d.ValidateurName, opt => opt.MapFrom(s => s.Validateur != null ? s.Validateur.FullName : null))
            .ForMember(d => d.DaysUntilDeadline, opt => opt.MapFrom(s => (s.DateLimite - DateTime.UtcNow).Days));

        // Declaration TVA
        CreateMap<DeclarationTVA, DeclarationTVADto>()
            .ForMember(d => d.LignesFactures, opt => opt.MapFrom(s => s.LignesFactures))
            .ReverseMap();
        CreateMap<TVALigneFacture, TVALigneFactureDto>().ReverseMap();

        // Declaration IS
        CreateMap<DeclarationIS, DeclarationISDto>().ReverseMap();

        // Declaration CNSS
        CreateMap<DeclarationCNSS, DeclarationCNSSDto>().ReverseMap();

        // Payroll
        CreateMap<PayrollPeriod, PayrollPeriodDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.RaisonSociale : null))
            .ForMember(d => d.Year, opt => opt.MapFrom(s => s.Annee))
            .ForMember(d => d.Month, opt => opt.MapFrom(s => s.Mois))
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Statut))
            .ForMember(d => d.TotalEmployees, opt => opt.MapFrom(s => s.PaySlips.Count))
            .ForMember(d => d.TotalBrutGlobal, opt => opt.MapFrom(s => s.TotalSalairesBruts))
            .ForMember(d => d.TotalNetPay, opt => opt.MapFrom(s => s.TotalNetAPayer))
            .ForMember(d => d.TotalChargesPatronales, opt => opt.MapFrom(s => s.TotalCotisationsPatronales));
        CreateMap<PaySlip, PaySlipDto>()
            .ForMember(d => d.EmployeeName, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
            .ForMember(d => d.Matricule, opt => opt.MapFrom(s => s.Employee != null ? s.Employee.MatriculeEmployeur : null))
            .ForMember(d => d.SalaireBrut, opt => opt.MapFrom(s => s.SalaireBrutGlobal))
            .ForMember(d => d.IR, opt => opt.MapFrom(s => s.IRRetenu))
            .ForMember(d => d.SalaireNet, opt => opt.MapFrom(s => s.SalaireNetAPayer))
            .ForMember(d => d.CNSSPatronal, opt => opt.MapFrom(s => s.CotisationPatronaleCNSS))
            .ForMember(d => d.AMOPatronal, opt => opt.MapFrom(s => s.CotisationPatronaleAMO))
            .ForMember(d => d.TotalChargesPatronales, opt => opt.MapFrom(s => s.CotisationPatronaleCNSS + s.CotisationPatronaleAMO + s.FormationProfessionnelle));

        // Document
        CreateMap<Document, DocumentDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.RaisonSociale : null))
            .ForMember(d => d.UploadedByUserName, opt => opt.MapFrom(s => s.UploadedByUser != null ? s.UploadedByUser.FullName : null));

        // Notification
        CreateMap<Notification, NotificationDto>();

        // Task
        CreateMap<TaskItem, TaskItemDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.RaisonSociale : null))
            .ForMember(d => d.AssignedToUserName, opt => opt.MapFrom(s => s.AssignedToUser != null ? s.AssignedToUser.FullName : null))
            .ForMember(d => d.AssignedByUserName, opt => opt.MapFrom(s => s.AssignedByUser != null ? s.AssignedByUser.FullName : null))
            .ForMember(d => d.CommentCount, opt => opt.MapFrom(s => s.Comments.Count));

        // ═══════════════════════════════════════════════════════════════
        //  DOCUMENTS COMMERCIAUX
        // ═══════════════════════════════════════════════════════════════

        // Ligne Document Commercial
        CreateMap<LigneDocumentCommercial, LigneDocumentCommercialDto>()
            .ForMember(d => d.ServiceContratLibelle, opt => opt.MapFrom(s => s.ServiceContrat != null ? s.ServiceContrat.LibelleService : null));
        CreateMap<LigneDocumentCommercialCreateDto, LigneDocumentCommercial>();

        // Devis
        CreateMap<Devis, DevisDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.RaisonSociale : null))
            .ForMember(d => d.CommercialName, opt => opt.MapFrom(s => s.Commercial != null ? s.Commercial.FullName : null))
            .ForMember(d => d.NombreLignes, opt => opt.MapFrom(s => s.Lignes.Count));
        CreateMap<DevisCreateDto, Devis>()
            .ForMember(d => d.Lignes, opt => opt.Ignore()); // Lignes handled manually

        // Bon de Commande
        CreateMap<BonCommande, BonCommandeDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.RaisonSociale : null))
            .ForMember(d => d.DevisReference, opt => opt.MapFrom(s => s.Devis != null ? s.Devis.Reference : null));

        // Contrat Commercial
        CreateMap<ContratCommercial, ContratCommercialDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.RaisonSociale : null))
            .ForMember(d => d.BonCommandeReference, opt => opt.MapFrom(s => s.BonCommande != null ? s.BonCommande.Reference : null))
            .ForMember(d => d.DevisReference, opt => opt.MapFrom(s => s.Devis != null ? s.Devis.Reference : null))
            .ForMember(d => d.NombreFactures, opt => opt.MapFrom(s => s.Factures.Count))
            .ForMember(d => d.TotalFacture, opt => opt.MapFrom(s => s.Factures.Sum(f => f.MontantTTC)));

        // Facture
        CreateMap<Facture, FactureDto>()
            .ForMember(d => d.ClientName, opt => opt.MapFrom(s => s.Client != null ? s.Client.RaisonSociale : null))
            .ForMember(d => d.ContratReference, opt => opt.MapFrom(s => s.Contrat != null ? s.Contrat.Reference : null));

        // Paiement
        CreateMap<Paiement, PaiementDto>()
            .ForMember(d => d.FactureReference, opt => opt.MapFrom(s => s.Facture != null ? s.Facture.Reference : null));
        CreateMap<PaiementCreateDto, Paiement>();

        // Historique
        CreateMap<DocumentCommercialHistorique, DocumentCommercialHistoriqueDto>();
    }
}
