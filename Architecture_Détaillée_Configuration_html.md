# Cahier des Charges Fonctionnel

**Gestion Intégrée d';un Cabinet Fiduciaire Marocain (Architecture .NET Core 9)**

Document préparé pour: MetaCompta

Version: 2.0

Date: 2026-03-15

Confidential

## Table des Matières

1.  [1\. Présentation du Projet](#section1)
    1.  [1.1. Contexte](#section1.1)
    2.  [1.2. Objectif du Projet](#section1.2)
    3.  [1.3. Périmètre](#section1.3)
2.  [2\. Architecture Technique et Modèle de Déploiement](#section2)
    1.  [2.1. Structure Logicielle (Architecture 3-Tiers)](#section2.1)
    2.  [2.2. Pile Technologique](#section2.2)
    3.  [2.3. Périmètres du Projet (Back-Office, Extranet, Admin)](#section2.3)
    4.  [2.4. Modèle Économique et Facturation SaaS](#section2.4)
3.  [3\. Utilisateurs et Besoins](#section3)
    1.  [3.1. Profils Utilisateurs](#section3.1)
    2.  [3.2. Nombre d'Utilisateurs par Licence](#section3.2)
4.  [4\. Besoins Fonctionnels Détaillés](#section4)
    1.  [4.1. Gestion des Clients](#section4.1)
    2.  [4.2. Déclarations Fiscales](#section4.2)
        1.  [4.2.1. Déclaration TVA](#section4.2.1)
        2.  [4.2.2. Déclaration IS](#section4.2.2)
        3.  [4.2.3. Déclaration IR](#section4.2.3)
    3.  [4.3. Déclarations Sociales](#section4.3)
        1.  [4.3.1. Déclaration CNSS](#section4.3.1)
        2.  [4.3.2. IR Salaires (Retenue à la Source)](#section4.3.2)
    4.  [4.4. Gestion des Employés (Clients du Cabinet)](#section4.4)
    5.  [4.5. Gestion de la Paie](#section4.5)
    6.  [4.6. Gestion Documentaire](#section4.6)
    7.  [4.7. Notifications et Alertes](#section4.7)
    8.  [4.8. Portail Client (Extranet)](#section4.8)
    9.  [4.9. Tableaux de Bord et Rapports](#section4.9)
    10.  [4.10. Gestion des Utilisateurs et des Droits d'Accès](#section4.10)
    11.  [4.11. Gestion des Tâches](#section4.11)
5.  [5\. Exigences Non Fonctionnelles](#section5)
    1.  [5.1. Performance](#section5.1)
    2.  [5.2. Sécurité](#section5.2)
    3.  [5.3. Disponibilité](#section5.3)
    4.  [5.4. Ergonomie et Localisation Multilingue](#section5.4)
    5.  [5.5. Compatibilité](#section5.5)
    6.  [5.6. Conformité Légale](#section5.6)
6.  [6\. Cas d'Utilisation](#section6)
    1.  [6.1. Cas d'Utilisation: Traiter une Déclaration TVA Mensuelle](#section6.1)
    2.  [6.2. Cas d'Utilisation: Valider et Soumettre une Déclaration](#section6.2)
    3.  [6.3. Cas d'Utilisation: Préparer la Paie Mensuelle](#section6.3)
    4.  [6.4. Cas d'Utilisation: Le Client Consulte Ses Déclarations](#section6.4)
7.  [7\. Planification et Phases](#section7)
8.  [8\. Critères d'Acceptation](#section8)
9.  [9\. Glossaire](#section9)
10.  [10\. Annexes](#section10)
     1.  [10.1. Annexe A: Exemples de Calculs](#section10.1)
     2.  [10.2. Annexe B: Taux et Plafonds Applicables (2026) & Référentiels Dynamiques](#section10.2)

## 1\. Présentation du Projet

Ce document détaille les exigences fonctionnelles et techniques pour le développement de MetaCompta, une solution numérique complète conçue pour rationaliser les opérations de gestion des cabinets fiduciaires au Maroc.

### 1.1. Contexte

Les cabinets fiduciaires marocains sont confrontés à des défis importants dans leurs opérations quotidiennes, notamment :

-   Gestion manuelle fastidieuse des déclarations fiscales et sociales.
-   Risques d'erreurs de calcul et de non-respect des délais.
-   Difficulté à suivre simultanément plusieurs clients.
-   Canaux de communication inefficaces avec les clients.
-   Perte de temps sur les tâches administratives répétitives.
-   Manque de traçabilité et d'historique des opérations.

### 1.2. Objectif du Projet

MetaCompta vise à être une solution numérique complète pour les cabinets fiduciaires, conçue pour :

-   Automatiser les processus de déclarations fiscales et sociales.
-   Centraliser toutes les informations clients sur une plateforme unique et sécurisée.
-   Sécuriser la gestion des documents et données sensibles.
-   Optimiser la productivité de l'équipe et le temps de travail.
-   Améliorer la communication et la collaboration avec les clients.
-   Assurer la conformité légale et réglementaire avec les lois marocaines.

### 1.3. Périmètre

La phase initiale de MetaCompta inclura les fonctionnalités suivantes :

-   Gestion complète des clients (dossiers d'entreprise).
-   Déclarations fiscales (TVA, IS, IR).
-   Déclarations sociales (CNSS, AMO).
-   Gestion du personnel des clients.
-   Traitement de la paie.
-   Gestion documentaire.
-   Portail client en ligne (Extranet).
-   Notifications et rappels automatiques.
-   Reporting et tableaux de bord.
-   Gestion des tâches.
-   Gestion granulaire des utilisateurs et des droits d'accès.
-   Localisation multilingue (Français, Arabe, Anglais).

Les fonctionnalités suivantes sont explicitement exclues de cette phase initiale mais sont prévues pour des développements futurs :

-   Comptabilité générale complète.
-   Gestion des immobilisations.
-   Facturation pour les clients propres du cabinet fiduciaire.

## 2\. Architecture Technique et Modèle de Déploiement

Cette section décrit la fondation technique obligatoire et la stratégie de déploiement pour la solution MetaCompta, garantissant un système robuste, évolutif et maintenable.

### 2.1. Structure Logicielle (Architecture 3-Tiers)

Le système adhérera strictement à un modèle architectural à 3-tiers, favorisant la séparation des préoccupations, l'évolutivité et la maintenabilité :

-   **Couche Présentation (Presentation Layer) :** Responsable de l'interaction utilisateur et de l'affichage des données. Cette couche comprendra principalement les interfaces utilisateur pour les projets Back-Office, Extranet et Admin. Elle communiquera avec la Couche Métier/Façade.
-   **Couche Métier/Façade (Business/Facade Layer) :** Encapsule la logique métier, les règles et les workflows. Elle agit comme un intermédiaire entre les couches Présentation et Données, exposant une interface simplifiée (Façade) à la Couche Présentation tout en orchestrant des opérations complexes impliquant la Couche Données. Cette couche gérera toutes les validations, les calculs (par exemple, fiscaux, de paie) et la gestion des workflows.
-   **Couche Données (Data Layer) :** Gère le stockage, la récupération et la persistance des données. Cette couche interagit directement avec la base de données, en abstrayant les détails d'accès aux données de la Couche Métier. Elle comprendra principalement le contexte Entity Framework Core 9, les modèles et les dépôts.

### 2.2. Pile Technologique

La pile technologique obligatoire pour le développement de MetaCompta est la suivante :

-   **Framework Backend :** ASP.NET Core 9. Ce framework sera utilisé pour construire les API robustes et les composants de logique métier à travers tous les périmètres du projet.
-   **Object-Relational Mapper (ORM) :** Entity Framework Core 9. Cela facilitera l'accès aux données et les opérations de persistance au sein de la Couche Données, assurant une interaction efficace avec la base de données sous-jacente.
-   **Base de Données :** Un système de gestion de base de données relationnelle (par exemple, SQL Server, PostgreSQL, MySQL) compatible avec Entity Framework Core 9 sera choisi pendant la phase de conception, en privilégiant la performance et l'évolutivité.
-   **Client-Side:** HTML5, CSS3, JavaScript avec un framework moderne (ex: React, Angular, Vue.js) pour des interfaces utilisateur dynamiques et réactives.

### 2.3. Périmètres du Projet (Back-Office, Extranet, Admin)

La solution MetaCompta sera structurée en trois projets distincts mais interconnectés, chacun servant une base d'utilisateurs spécifique et un ensemble de fonctionnalités :

#### Projet Back-Office :

-   **Objectif :** L'application interne principale pour les employés du cabinet fiduciaire (Gérant, Manager, Assistant Comptable).
-   **Fonctionnalités :** Gestion complète des clients, traitement de toutes les déclarations fiscales et sociales, gestion des employés et de la paie des clients, gestion documentaire interne, notifications, rapports internes, et gestion des tâches.
-   **Accès Utilisateur :** Restreint au personnel authentifié du cabinet.

#### Projet Extranet :

-   **Objectif :** Un portail web sécurisé pour les clients du cabinet fiduciaire (Client Final).
-   **Fonctionnalités :** Tableaux de bord spécifiques au client, consultation des déclarations soumises et des fiches de paie, téléchargement/téléchargement sécurisé de documents, outils de communication et accès aux informations de base de l'entreprise.
-   **Accès Utilisateur :** Restreint aux utilisateurs clients authentifiés associés à un cabinet et à une entreprise cliente spécifique.
-   **Exemple d'URL :** [https://portail.metacompta\_7724.ma](https://portail.metacompta.ma) ou [https://portail.metacompta\_1034.ma](https://portail.metacompta.ma).

#### Projet Admin (Administration SaaS) :

-   **Objectif :** Un portail d'administration dédié à la gestion de la plateforme SaaS elle-même, y compris les abonnements, les licences utilisateur et la facturation pour les cabinets fiduciaires.
-   **Fonctionnalités :**
    -   **Gestion des Cabinets :** Intégration de nouveaux cabinets fiduciaires, configuration de leur accès au système et de leurs paramètres initiaux.
    -   **Gestion des Abonnements :** Création, modification et annulation de plans d'abonnement (par exemple, Starter, Professionnel, Entreprise).
    -   **Gestion des Licences Utilisateur :** Suivi et attribution des licences utilisateur (par cabinet, par accès client) en fonction des abonnements actifs.
    -   **Facturation et Création de Factures :** Génération automatique de factures récurrentes pour les cabinets en fonction de leurs plans d'abonnement et de leur utilisation réelle (par exemple, nombre d'utilisateurs actifs, nombre d'entreprises clientes gérées).
    -   **Traitement des Paiements :** Intégration avec une passerelle de paiement pour la collecte des frais d'abonnement.
    -   **Configuration Globale :** Gestion des paramètres système, des taux légaux (TVA, CNSS, IR), des barèmes, plafonds, catégories de documents, étapes de workflow, et autres paramètres globaux, ainsi que la gestion des textes de localisation pour l'interface.
    -   **Audit et Suivi :** Journaux d'audit au niveau de la plateforme et surveillance de la santé du système.
-   **Accès Utilisateur :** Restreint aux administrateurs de la plateforme MetaCompta.

### 2.4. Modèle Économique et Facturation SaaS

La solution MetaCompta fonctionnera sur un modèle Software-as-a-Service (SaaS) avec des options d'abonnement flexibles, gérées entièrement via le projet Admin.

#### Structure d'Abonnement :

-   **Par Utilisateur par Cabinet :** Les cabinets fiduciaires seront facturés en fonction du nombre de leurs utilisateurs internes (Gérant, Manager, Assistant) ayant accès au projet Back-Office.
-   **Par Client Cabinet (Entreprise Gérée) :** Un composant supplémentaire de l'abonnement sera basé sur le nombre d'entreprises clientes gérées par le cabinet fiduciaire au sein de MetaCompta.
-   **Accès Utilisateur Extranet :** Le nombre d'utilisateurs extranet côté client (par exemple, jusqu'à 10 par entreprise cliente selon le périmètre initial) sera régi par l'abonnement global de l'entreprise cliente, avec des options pour des licences supplémentaires.

#### Gestion Flexible des Plans :

-   Le projet Admin doit permettre aux administrateurs de MetaCompta de créer, modifier ou retirer facilement des plans d'abonnement sans nécessiter de modifications de code.
-   Les cabinets fiduciaires, via une interface dédiée (ou via le support MetaCompta), doivent pouvoir mettre à niveau, rétrograder ou personnaliser leurs plans d'abonnement de manière dynamique.
-   Le système doit recalculer automatiquement la facturation lors des changements de plan, en appliquant des frais ou des crédits au prorata le cas échéant.

#### Facturation Automatisée :

-   Le projet Admin générera des factures récurrentes (par exemple, mensuelles, annuelles) basées sur les plans d'abonnement actifs et le nombre d'utilisateurs/clients pour chaque cabinet fiduciaire.
-   Les factures détailleront les services rendus, le modèle de tarification et la période applicable.
-   L'intégration avec une passerelle de paiement pour les prélèvements automatiques ou les paiements par carte de crédit sera requise.

## 3\. Utilisateurs et Besoins

Le système MetaCompta est conçu pour répondre aux besoins de divers profils d'utilisateurs au sein d'un cabinet fiduciaire et de son écosystème client, chacun ayant des rôles, des responsabilités et des exigences distincts.

### 3.1. Profils Utilisateurs

Le système définit quatre profils d'utilisateurs principaux, avec une distinction claire entre le personnel interne du cabinet et les utilisateurs externes (clients des clients du cabinet) :

-   **Gérant de Cabinet (SuperAdmin du cabinet) :**
    -   **Description :** Propriétaire ou directeur du cabinet fiduciaire. Il s'agit d'un employé du cabinet.
    -   **Besoins :** Vue d'ensemble complète de tous les clients, suivi des performances de l'équipe, validation des déclarations critiques, gestion des utilisateurs et des droits d'accès au sein du cabinet, statistiques et rapports globaux, configuration des paramètres spécifiques au cabinet.
    -   **Attentes :** Tableau de bord avec des indicateurs clés de performance, alertes pour les cas critiques, rapports exportables, contrôle total sur le système pour son cabinet.
-   **Responsable/Manager :**
    -   **Description :** Responsable d'un portefeuille de clients au sein du cabinet. Il s'agit d'un employé du cabinet.
    -   **Besoins :** Gestion des clients assignés, supervision et validation des déclarations avant soumission, attribution de tâches aux assistants, suivi des délais.
    -   **Attentes :** Interface claire et organisée, notifications de validation, historique complet des actions, outils de collaboration.
-   **Assistant(e) Comptable :**
    -   **Description :** Personnel opérationnel du cabinet. Il s'agit d'un employé du cabinet.
    -   **Besoins :** Saisie des déclarations, téléchargement et classification des documents, préparation des fiches de paie, saisie des données des employés (des clients), mise à jour des informations clients.
    -   **Attentes :** Interface simple et intuitive, calculs automatiques, modèles préremplis, aide contextuelle, accès restreint aux fonctions sensibles.
-   **Client Final (Entreprise Cliente - Utilisateur Extranet) :**
    -   **Description :** Gérant ou comptable de l'entreprise cliente servie par le cabinet. Il s'agit d'un utilisateur externe au cabinet.
    -   **Besoins :** Consultation de leurs déclarations, téléchargement des fiches de paie, consultation des délais, téléchargement de documents vers le cabinet, demande d'attestations.
    -   **Attentes :** Portail simple et accessible, informations en temps réel, historique complet, aucune capacité de manipulation de données.

### 3.2. Nombre d'Utilisateurs par Licence

Pour chaque client fiduciaire, les allocations d'utilisateurs par défaut suivantes sont fournies comme base pour la licence, avec une flexibilité de personnalisation via le projet d'administration SaaS :

-   **Utilisateurs internes du Cabinet :** Jusqu'à 3 utilisateurs de gestion (Gérant + Managers/Assistants).
-   **Utilisateurs du Portail Client (Extranet) :** Jusqu'à 10 utilisateurs du portail client (pour l'accès par les entreprises clientes).
-   **Total :** Cela représente 13 utilisateurs simultanés par licence de cabinet standard, avec des options évolutives.

## 4\. Besoins Fonctionnels Détaillés

Cette section développe les fonctionnalités de base requises pour MetaCompta, couvrant tous les aspects de la gestion fiduciaire dans un contexte marocain.

### 4.1. Gestion des Clients

Le système doit centraliser toutes les informations relatives aux entreprises clientes du cabinet fiduciaire, avec des tables enrichies pour une flexibilité maximale.

#### A. Création de Client

Les utilisateurs doivent pouvoir créer des dossiers clients complets en saisissant :

-   **Informations d'Identification :**
    -   Raison sociale (Nom légal)
    -   ICE (Identifiant Commun de l'Entreprise) - 15 chiffres, unique.
    -   IF (Identifiant Fiscal) - unique.
    -   RC (Registre de Commerce) - unique.
    -   N° CNSS entreprise (Numéro CNSS de l'entreprise) - unique.
    -   N° Patente
    -   Date de Début d'Activité
    -   Code d'Activité (référentiel configurable)
    -   Forme Juridique (SARL, SA, SNC, etc. - référentiel configurable)
-   **Informations Légales :**
    -   Secteur d'activité (référentiel configurable)
    -   Date de création
    -   Capital social
    -   Adresse du Siège Social
    -   Ville du Siège Social (référentiel configurable)
    -   Numéro de Téléphone Principal
    -   Adresse E-mail Principale
    -   Contact Principal (Nom, Fonction, Téléphone, Email)
-   **Informations de Service :**
    -   Manager Principal du cabinet assigné à ce client.
    -   Assistant(s) assigné(s) à ce client.
    -   Type de TVA (Mensuelle, Trimestrielle - configurable par client, valeur par défaut du référentiel).
    -   Date de clôture d'exercice (configurable par client, par défaut 31/12).
    -   Statut de la société (Active, Dissoute, Dormante - référentiel configurable).
    -   Date de début de contrat avec le cabinet.
    -   Notes internes.
    -   Champs personnalisés configurables (ex: URL du site web, compte bancaire par défaut pour les paiements).

#### B. Consultation et Modification

Les utilisateurs doivent pouvoir :

-   Rechercher un client par raison sociale, ICE, IF, RC, ou toute combinaison de critères.
-   Visualiser le dossier client complet, y compris les informations détaillées et les déclarations associées.
-   Modifier les informations client (avec traçabilité complète de l'historique des modifications).
-   Désactiver un client (suppression logique sans suppression réelle) : le client ne sera plus actif pour les nouvelles opérations mais son historique sera conservé.
-   Archiver un client désactivé après une période définie (configurable) pour des raisons de conformité légale.

#### C. Vue d'Ensemble Client (Dashboard Client)

Pour chaque client, le système affichera :

-   Un résumé des informations clés.
-   Statut actif/inactif.
-   Nombre d'employés.
-   Dernières déclarations soumises.
-   Documents récents.
-   Délais à venir.
-   Historique des actions liées à ce client.
-   Indicateurs clés personnalisables (ex: Chiffre d'Affaires annuel, total TVA payée).

#### D. Règles Métier

-   Un client ne peut être supprimé (même logiquement) s'il a des déclarations ou des employés associés, garantissant l'intégrité référentielle.
-   ICE, IF, RC, N° CNSS entreprise doivent être des identifiants uniques à travers le système.
-   Seuls les utilisateurs avec des profils Manager ou Gérant peuvent créer ou modifier des clients. Les Assistants peuvent créer des brouillons mais la validation finale pourrait revenir à un Manager.
-   Toutes les modifications apportées aux données client doivent être enregistrées pour des raisons d'audit (qui, quand, ce qui a été modifié, ancienne et nouvelle valeur).
-   Les types de TVA (Mensuelle, Trimestrielle) sont configurables dynamiquement pour chaque client.

#### E. Architecture Contrat → Services (Single Source of Truth)

**Invariant fondamental :** Les Services Fiduciaires d'un Client sont **exclusivement** gérés via le cycle de vie du Contrat Commercial. Le ContratCommercial validé est la **source unique de vérité** pour les services assignés à un client.

**Règles :**

1. **Les services ne sont JAMAIS créés/modifiés manuellement** lors de la création ou modification d'un client.
2. **Un client ne peut pas avoir de services en dehors d'un contrat.**
3. **Lors de la validation d'un contrat** (`ValiderContratAsync`), les services sont automatiquement activés via `ActiverServicesFromContratAsync` :
   - Chaque ligne de type "Service" dans le contrat → crée ou réactive un `ServiceContrat` avec des **smart defaults** :
     - **Catégorie** : détectée automatiquement (Fiscal, Social, Comptable, Juridique, Conseil)
     - **Fréquence** : défaut intelligent basé sur le type de service + régime TVA du client
     - **Délais** : calendrier fiscal marocain (TVA=J20/M+1, IS=J31/M+3, IR=J28/M+2, CNSS=J10/M+1)
     - **Exercice** : dates calculées depuis le mois de clôture du client
4. **Les services existants absents du nouveau contrat sont désactivés** (Actif=false, DateFin=now).
5. **Les vues** (Create, Edit, Details) affichent les services en **lecture seule** avec un bandeau indiquant la gestion par contrat.

**Flux :**
```
Création Client → Pas de services
     ↓
Commercial → Créer Devis → Convertir en Contrat → Signer → Valider
     ↓
ActiverServicesFromContratAsync → ServiceContrats créés avec smart defaults
     ↓
Client.ServicesFiduciaires = reflet exact du contrat validé
```

### 4.2. Déclarations Fiscales

Le système automatisera la préparation et le suivi des déclarations fiscales obligatoires conformément à la législation fiscale marocaine (CGI). Les workflows de déclaration sont configurables et les taux sont dynamiques.

#### A. Workflow de Déclaration (Applicable à toutes les déclarations)

Chaque type de déclaration suivra un workflow configurable, avec des statuts et des transitions gérées par des permissions. Le workflow par défaut est :

1.  **Brouillon (Draft) :** Création automatique ou manuelle, attente de saisie.
2.  **À Compléter (To Complete) :** Saisie des données par un Assistant.
3.  **Complétée (Completed) :** Assistant a terminé la saisie.
4.  **En Révision (Under Review) :** Manager est notifié pour vérification.
5.  **Validée (Validated) :** Manager a approuvé.
6.  **Soumise (Submitted) :** Manager a effectué la télé-déclaration et confirmé.
7.  **Refusée (Rejected) :** Manager ou Gérant a refusé la déclaration (nécessite une raison et retour au statut "Brouillon" ou "À Compléter").

Les transitions entre ces statuts sont contrôlées par des droits d'accès granulaires. Par exemple, seule une personne avec la permission "Declaration.Validate" peut passer une déclaration de "Complétée" à "Validée". Des notifications automatiques sont générées lors des changements de statut (ex: "Déclaration Complétée" -> notification au Manager).

Le système permettra au Gérant de cabinet de configurer des workflows personnalisés pour chaque type de déclaration, y compris l'ajout de statuts intermédiaires ou la modification des rôles responsables de chaque transition.

#### 4.2.1. Déclaration TVA

Ceci couvre les déclarations TVA mensuelles ou trimestrielles pour les sociétés assujetties.

-   **Processus :**
    -   **Création Automatique :** Le système crée automatiquement les déclarations mensuelles/trimestrielles pour chaque client, en pré-remplissant la période et la date limite. Le statut initial est "Brouillon" ou "À Compléter". La période de déclaration (mensuelle/trimestrielle) est configurée par client.
    -   **Saisie des Données :** L'Assistant saisit :
        -   Chiffre d'Affaires HT aux taux de 20%, 14%, 10%, 7% (TVA calculée automatiquement à partir des taux configurables dans le référentiel des taux TVA).
        -   Chiffre d'Affaires exonéré.
        -   TVA récupérable : TVA sur les charges (achats, fournitures, services), TVA sur les immobilisations (équipements, véhicules).
        -   Autres : Crédit de TVA du mois précédent (pré-rempli automatiquement si disponible).
    -   **Calculs Automatiques :** Le système calcule :
        -   TVA Collectée Totale = Σ TVA 20% + 14% + 10% + 7%
        -   TVA Récupérable Totale = TVA Charges + TVA Immobilisations
        -   TVA Nette = TVA Collectée - TVA Récupérable - Crédit Précédent
        -   Si TVA Nette > 0 : TVA à payer.
        -   Si TVA Nette < 0 : Crédit de TVA (reporté au mois suivant).
    -   **Validation et Soumission :** Suivi du workflow de déclaration (ex: "Complétée" par Assistant, "Validée" par Manager, "Soumise" après télé-déclaration).
-   **Règles Métier :**
    -   Déclarations mensuelles : Dues avant le 30 du mois suivant.
    -   Déclarations trimestrielles : Dues avant le 30 du mois suivant le trimestre.
    -   Alertes : 7 jours, 3 jours, et le jour de la date limite (valeurs configurables).
    -   Verrouillage de Modification : Les déclarations ne peuvent pas être modifiées après soumission.
    -   Historique : Toutes les versions (brouillons, validées, soumises) doivent être conservées.
    -   Les taux de TVA sont configurables via le module Admin ou par le Gérant pour les cas spécifiques.
-   **Livraisons :**
    -   Déclaration PDF pré-remplie et conforme aux modèles de la DGI.
    -   Tableau récapitulatif mensuel/annuel.
    -   Export Excel pour la télé-déclaration SIMPL DGI.

#### 4.2.2. Déclaration IS (Impôt sur les Sociétés)

Ceci couvre la déclaration annuelle de l'impôt sur les sociétés.

-   **Informations à Saisir :** Année fiscale (début/fin), résultat comptable, réintégrations fiscales, déductions fiscales, résultat fiscal, déficits antérieurs à imputer, taux IS applicable (10%, 20%, 31% - configurable et gérable dynamiquement).
-   **Calculs Automatiques :**
    -   Résultat Fiscal = Résultat Comptable + Réintégrations - Déductions
    -   Base Imposable = Résultat Fiscal - Déficits Antérieurs
    -   IS Dû = Base Imposable × Taux IS (selon le barème IS dynamique)
    -   IS Net = IS Dû - Acomptes Versés
-   **Acomptes Provisionnels :** Le système calcule et rappelle les 4 paiements (31 mars, 30 juin, 30 septembre, 31 décembre), chacun représentant 25% de l'IS de l'année précédente. Les dates et pourcentages sont configurables.
-   **Date Limite :** 3 mois après la clôture de l'exercice fiscal (par exemple, clôture au 31 décembre → date limite au 31 mars).
-   **Règles Métier :** Le taux d'IS est configurable dans le référentiel et peut être spécifique à certains secteurs ou montants de bénéfices.

#### 4.2.3. Déclaration IR (Impôt sur le Revenu)

Ceci couvre la déclaration annuelle de l'impôt sur le revenu pour les entreprises individuelles et les professions libérales.

-   **Types d'IR Gérés :** IR Professionnel (revenus professionnels), IR Salaires (pour les gérants non salariés), IR Agricole.
-   **Informations à Saisir :** Revenu net par catégorie, dépenses déductibles, abattements applicables, réductions et crédits d'impôt.
-   **Application du Barème Progressif :** Le système applique automatiquement le barème d'impôt progressif marocain (configurable dans le référentiel) :
    -   Tranche 1 : 0 à 30 000 DH → 0%
    -   Tranche 2 : 30 001 à 50 000 DH → 10%
    -   Tranche 3 : 50 001 à 60 000 DH → 20%
    -   Tranche 4 : 60 001 à 80 000 DH → 30%
    -   Tranche 5 : 80 001 à 180 000 DH → 34%
    -   Tranche 6 : > 180 000 DH → 38%
-   **Date Limite :** 30 avril de l'année N+1.
-   **Règles Métier :** Le barème IR, les abattements et les réductions sont configurables dynamiquement.

### 4.3. Déclarations Sociales

Cette section détaille la gestion des déclarations sociales obligatoires, avec une forte automatisation et conformité aux réglementations marocaines.

#### 4.3.1. Déclaration CNSS (Caisse Nationale de Sécurité Sociale)

Ceci couvre la déclaration mensuelle obligatoire des salaires et des cotisations sociales à la CNSS.

-   **Processus :**
    -   **Sources de Données :** Le système récupère automatiquement la liste des employés actifs, les salaires bruts mensuels (issus de la paie), les nouvelles embauches et les départs pour le mois concerné.
    -   **Calculs Automatiques :** Pour chaque employé, basés sur un plafond CNSS de 6 000 DH (valeur configurable et stockée en base de données) :
        -   Salaire Brut = Salaire de Base + Primes + Indemnités
        -   Plafond CNSS = 6 000 DH (valeur référencée)
        -   Base de Cotisation = Min(Salaire Brut, Plafond CNSS)
        -   Cotisation Salariale = Base de Cotisation × 6.74% (taux référencé)
        -   Cotisation Patronale = Base de Cotisation × 20.48% (taux référencé)
        -   AMO Salariale = Base de Cotisation × 2.26% (taux référencé)
        -   AMO Patronale = Base de Cotisation × 4.11% (taux référencé)
        -   Formation Professionnelle = Salaire Brut × 1.6% (taux référencé)
    -   **Bordereau Récapitulatif :** Le système génère une liste nominative (N° CNSS, Nom, Salaire, Cotisations), les salaires déclarés totaux, les cotisations totales employé/employeur et le montant total à payer.
    -   **Export :** Fichier XML pour la télé-déclaration DAMANCOM, PDF du relevé, rapport récapitulatif mensuel.
-   **Règles Métier :**
    -   Date limite : 10 du mois suivant.
    -   Pénalités pour paiement tardif calculées automatiquement si soumission après le 10 (taux et modalités configurables).
    -   Alertes : 5 jours avant la date limite (nombre de jours configurable).
    -   Le traitement est bloqué si la paie du mois n'a pas été validée pour le client concerné.
    -   Les taux et plafonds (CNSS, AMO, FP) sont dynamiquement configurables et stockés en base de données.

#### 4.3.2. IR Salaires (Retenue à la Source)

Ceci couvre la déclaration mensuelle de l'impôt sur le revenu retenu sur les salaires des employés.

-   **Calcul par Employé :**
    -   Revenu Brut Imposable Annuel : Salaire Brut Mensuel × 12 - Cotisations Sociales (CNSS, AMO, CIMR) = Revenu Net Imposable Annuel.
    -   Déductions pour Charges de Famille : Déduction = Nombre d'enfants à charge (référencé par l'employé) × 360 DH/an (plafond de 6 enfants), plafonné à 2 160 DH/an. Ces valeurs sont configurables.
    -   Application du Barème Progressif : Le système applique automatiquement le barème progressif d'IR (configurable, voir 4.2.3).
    -   IR Mensuel : IR Annuel / 12 = IR Mensuel à retenir.
-   **Bordereau de Versement :** Liste les employés avec l'IR retenu et le montant total d'IR à verser à l'État.
-   **Date Limite :** 10 du mois suivant.
-   **Règles Métier :** Les barèmes IR, les montants des déductions et plafonds sont configurables et stockés en base de données.

### 4.4. Gestion des Employés (Clients du Cabinet)

Le système doit gérer les données du personnel pour toutes les entreprises clientes du cabinet fiduciaire. Ces employés sont ceux des clients du cabinet et sont distincts des employés du cabinet fiduciaire lui-même.

#### A. Fiche Employé Complète et Enrichie

-   **Informations d'Identité :** CIN (unique), Nom, Prénom, Date et Lieu de Naissance, Adresse Complète, Téléphone (portable et fixe), Email, Situation Familiale (Marié, Célibataire, Divorcé - référentiel configurable), Nombre d'Enfants à Charge (pour les calculs IR), Date de Mariage, Sexe.
-   **Informations Professionnelles :**
    -   Client (Lien obligatoire vers l'entreprise cliente).
    -   N° CNSS (unique par client, vérification de l'unicité globale).
    -   Date d'Embauche, Date de Sortie (si applicable).
    -   Type de Contrat (CDI, CDD, Intérim, Stage - référentiel configurable).
    -   Durée du Contrat (pour CDD).
    -   Poste, Département/Service (référentiel configurable).
    -   Qualification, Catégorie socio-professionnelle.
    -   Matricule Employeur (numéro interne du client pour l'employé).
    -   Statut (Actif, Inactif, Suspendu - référentiel configurable).
-   **Informations de Rémunération :**
    -   Salaire de Base Mensuel.
    -   Primes fixes (Ancienneté, Responsabilité, Présence - configurables avec leurs règles de calcul).
    -   Indemnités (Transport, Représentation - configurables).
    -   Avantages en nature (Voiture, Logement - configurables).
    -   Taux Horaire (si applicable).
    -   Mode de Paiement (Virement bancaire, Espèces, Chèque - référentiel configurable).
-   **Informations Bancaires :** RIB complet (24 chiffres - format marocain), Nom de la Banque, Agence Bancaire.
-   **Autres :** Photo de l'employé, Contact d'urgence (Nom, Relation, Téléphone), Notes internes.

#### B. Gestion des Mouvements

Les utilisateurs peuvent :

-   Ajouter de nouveaux employés (embauche) avec une date d'entrée.
-   Modifier les informations (changement de poste, augmentation de salaire, changement d'adresse) avec traçabilité.
-   Gérer les absences et congés (types de congés configurables : maladie, annuel, sans solde, etc.) avec dates de début/fin.
-   Enregistrer les départs (démission, licenciement, retraite) avec une date de sortie et un motif (référentiel configurable).
-   Suspendre temporairement (maladie de longue durée, congé parental) avec motif et dates.

#### C. Documents Employé

Associer à chaque employé des documents numériques :

-   Copie CIN, CV, Contrat de travail signé, Avenants, Attestations CNSS, Certificat médical, Fiches de paie générées.
-   Possibilité d'ajouter des catégories de documents personnalisées.

#### D. Historique

Suivre et consulter l'historique de :

-   Historique des salaires (date d'effet, ancien/nouveau salaire).
-   Historique des postes.
-   Absences et congés pris.
-   Documents générés (attestations de travail, certificats de salaire).

#### E. Règles Métier

-   Un employé appartient à une seule entreprise cliente à la fois (pour un client fiduciaire donné).
-   Le N° CNSS doit être unique pour un client donné.
-   Impossible de supprimer un employé s'il a des fiches de paie existantes (suppression logique recommandée).
-   Seuls les employés inactifs (sortis, suspendus) peuvent être archivés.
-   Toutes les modifications génèrent une piste d'audit.
-   Les listes déroulantes (type de contrat, motif de départ, poste, département) sont gérables via des référentiels dynamiques.

### 4.5. Gestion de la Paie

Le système automatisera le calcul et la génération des fiches de paie en conformité avec le droit du travail marocain et les configurations dynamiques des référentiels.

#### Processus Mensuel

-   **Initialisation :** L'Assistant sélectionne le client et la période. Le système récupère tous les employés actifs du client et pré-remplit les salaires de base et les primes fixes (issues de la configuration de l'employé).
-   **Saisie des Variables :** Pour chaque employé, saisir les variables applicables : heures supplémentaires (25%, 50%, 100% - taux configurables), primes exceptionnelles, indemnités exceptionnelles, avances sur salaire, absences non rémunérées (en jours ou heures), congés sans solde.
-   **Calculs Automatiques :** Le système calcule pour chaque employé (avec les taux et plafonds provenant des référentiels dynamiques) :
    -   **Salaire Brut Global :** Salaire de Base + Primes Fixes + Primes Variables + Indemnités + Heures Supplémentaires + Avantages en nature.
    -   **Cotisations Sociales (parts salariales) :**
        -   CNSS Salariale = Min(Salaire Brut Global, Plafond CNSS) × Taux CNSS Salariale
        -   AMO Salariale = Min(Salaire Brut Global, Plafond CNSS) × Taux AMO Salariale
        -   CIMR/Retraite Complémentaire = selon le taux et la base définis par l'entreprise cliente (configurable).
    -   **Salaire Net Imposable :** Salaire Brut Global - CNSS Salariale - AMO Salariale - CIMR/Retraite.
    -   **IR sur Salaire :** Application automatique du barème progressif (voir 4.2.3) avec les déductions pour charges de famille et autres déductions légales (toutes valeurs configurables).
    -   **Salaire Net à Payer :** Salaire Brut Global - Somme de toutes les retenues (CNSS, AMO, CIMR, IR, Avances, Absences non rémunérées, etc.).
    -   **Cotisations Employeur :**
        -   CNSS Patronale = Min(Salaire Brut Global, Plafond CNSS) × Taux CNSS Patronale
        -   AMO Patronale = Salaire Brut Global × Taux AMO Patronale
        -   Formation Professionnelle (FP) = Salaire Brut Global × Taux FP
        -   Assurances Complémentaires (si configurées).
    -   **Coût Total Employeur :** Salaire Brut Global + Charges Patronales.
-   **Validation :** L'Assistant vérifie toutes les fiches. Le Manager valide l'intégralité de la paie. Le statut passe de "Brouillon" à "Validée".
-   **Génération des Livrables :** Le système génère automatiquement :
    -   Fiches de Paie Individuelles (PDF) : Format officiel avec en-tête de l'entreprise, infos employé, détails des gains, déductions, net à payer en chiffres et en lettres, cumuls annuels.
    -   Livre de Paie (Registre de Paie) : Registre obligatoire avec la liste mensuelle des employés, le résumé des éléments de paie et les totaux.
    -   Journal Comptable : Écritures comptables pour la paie (charges de personnel, charges sociales, dettes envers les organismes sociaux, dettes envers le personnel).
    -   Ordres de Virement Bancaire : Fichier CSV/Excel pour les virements, compatible avec les banques marocaines (RIB, montant net, nom, référence).
    -   Déclarations pré-remplies CNSS et IR Salaires pour le mois.

#### Règles Métier

-   Impossible de modifier une paie validée.
-   Possibilité de créer une version corrective (annule et remplace la version précédente) avec traçabilité.
-   Notification automatique aux employés (via le portail client) dès que les fiches de paie sont disponibles.
-   Archivage automatique de tous les documents de paie générés.
-   Les calculs doivent être conformes à la législation du travail marocaine et aux paramètres configurés dans les référentiels.
-   Les taux, plafonds et barèmes utilisés dans les calculs de paie (CNSS, AMO, IR, HS, etc.) doivent être entièrement configurables via le module Admin ou par le Gérant de cabinet, et stockés en base de données.

### 4.6. Gestion Documentaire

Le système doit centraliser et organiser tous les documents clients de manière sécurisée, y compris ceux liés aux "Dossiers clients", avec un accent sur la flexibilité et la traçabilité.

#### A. Téléchargement de Documents (Upload)

Les utilisateurs peuvent :

-   Télécharger des fichiers uniques ou multiples (fonctionnalité glisser-déposer).
-   Formats acceptés : PDF, Word, Excel, Images (JPG, PNG), ZIP, etc. (liste configurable).
-   Taille maximale : 50 MB par fichier (configurable).
-   Téléchargement en lot pour plusieurs fichiers.
-   Pour chaque document, l'utilisateur doit saisir : Titre, Catégorie (liste déroulante configurable), Date du Document, Client Concerné, Description Facultative, Mots-clés/Tags.

#### B. Catégories de Documents

Les catégories pré-définies, entièrement configurables via le module Admin ou par le Gérant, incluront (liste extensible) :

-   **Documents Légaux :** Statuts de société, PV d'AG, contrats commerciaux, baux commerciaux.
-   **Documents Fiscaux :** Déclarations TVA, déclarations IS, déclarations IR, attestations fiscales, certificats de conformité.
-   **Documents Sociaux :** Relevés CNSS, attestations CNSS, déclarations IR salaires, synthèses AMO.
-   **Documents Comptables :** Factures d'achat, factures de vente, relevés bancaires, rapprochements bancaires, pièces justificatives.
-   **Documents RH :** Contrats de travail, avenants, fiches de paie, attestations de travail, certificats médicaux.
-   **Documents Administratifs :** Certificat d'inscription, certificat négatif, patente, certificat CNSS, diverses autorisations.

#### C. Consultation et Recherche

Les utilisateurs peuvent :

-   Visualiser tous les documents pour un client spécifique.
-   Filtrer par catégorie, date, statut, utilisateur ayant téléchargé.
-   Rechercher par mot-clé dans le titre, la description ou les tags.
-   Recherche avancée multi-critères (par client, catégorie, période, type de fichier).
-   Vue liste ou vue miniature.
-   Prévisualisation sans téléchargement (pour PDF, images, documents texte).

#### D. Workflow de Validation Documentaire

Un workflow configurable pour la validation des documents :

1.  Document téléchargé → Statut "En Attente".
2.  Assistant/Manager examine.
3.  Validation → Statut "Validé".
4.  Rejet → Statut "Rejeté" avec motif (retour à l'utilisateur ayant téléchargé pour correction ou nouvelle version).

Les permissions contrôlent qui peut changer le statut. Des notifications sont générées.

#### E. Versioning

-   Si un document avec le même nom est téléchargé, une nouvelle version est créée.
-   L'historique des versions est conservé, permettant de revenir aux versions précédentes.
-   Indication claire de la version active actuelle.

#### F. Partage et Droits

-   Les documents sont accessibles en fonction du profil utilisateur et de l'appartenance au client.
-   Les utilisateurs clients ne voient que leurs propres documents et ceux que le cabinet a choisi de partager avec eux.
-   Possibilité de partager/départager un document avec le client (via l'Extranet).
-   Les actions de téléchargement sont enregistrées (qui, quand, quel document, de quelle adresse IP).

#### G. Archivage Automatique

-   Les documents plus anciens que X années (période configurable, ex: 10 ans selon les obligations légales marocaines) sont automatiquement archivés.
-   Les documents archivés restent accessibles mais sont marqués "Archivés".
-   La fonctionnalité de recherche inclut les documents archivés.

#### H. Règles Métier

-   Suppression logique uniquement (document marqué comme supprimé mais conservé). La suppression permanente est réservée au SuperAdmin avec une piste d'audit spécifique.
-   Catégorisation obligatoire pour chaque document.
-   Analyse antivirus automatique lors du téléchargement.
-   Stockage sécurisé et chiffré des documents.
-   Les types de fichiers acceptés, la taille maximale, et les périodes d'archivage sont configurables.

### 4.7. Notifications et Alertes

Le système doit informer les utilisateurs des événements et des délais importants, avec des configurations dynamiques.

#### Types de Notifications (Configurables)

-   **Délais de Déclaration :** 7 jours avant (info), 3 jours avant (avertissement), Jour J (urgent), Après la date limite (critique). Les délais sont configurables.
-   **Validations en Attente :** Déclaration complétée par l'assistant → notification au manager. Badge indiquant le nombre de validations en attente.
-   **Documents à Traiter :** Nouveaux documents téléchargés par le client, documents en attente de validation.
-   **Paie :** Rappel mensuel pour préparer la paie, paie complétée par l'assistant → notification au manager, fiches de paie disponibles → notification aux clients.
-   **Système :** Nouveau message du client, nouvelle tâche assignée, mises à jour importantes du système.

#### Modes de Notification (Configurables par utilisateur)

-   **Dans l'application :** Icône de cloche avec badge (compteur non lu).
-   **Email :** Pour les notifications importantes uniquement.
-   **Tableau de Bord :** Widget de notifications récentes.

#### Gestion des Notifications

Les utilisateurs peuvent :

-   Voir toutes les notifications (lues/non lues).
-   Marquer comme lu, marquer tout comme lu, supprimer une notification.
-   Configurer les préférences (activer/désactiver par type de notification, choisir le mode de réception).
-   Les modèles d'e-mail de notification sont configurables (texte, objet, expéditeur).

### 4.8. Portail Client (Extranet)

Le projet extranet fournira aux clients un accès sécurisé et en consultation seule à leurs données via une URL dédiée.

#### Accès

-   Connexion avec email et mot de passe.
-   Chaque entreprise cliente peut avoir jusqu'à 10 utilisateurs portail (configurable via l'administration SaaS).
-   URL Exemple : [https://portail.metacompta\_7724.ma](https://portail.metacompta.ma) ou [https://portail.metacompta\_1034.ma](https://portail.metacompta.ma).

#### Fonctionnalités Client

-   **Tableau de bord :** Résumé de l'entreprise, délais à venir, dernières déclarations, documents récemment téléchargés.
-   **Déclarations :** Liste de toutes les déclarations (TVA, IS, IR, CNSS), filtre par type/période/statut, téléchargement PDF, consultation de l'historique complet.
-   **Employés et Paie :** Liste des employés (sans les salaires détaillés), téléchargement des fiches de paie (PDF), consultation des cumuls annuels, attestations de travail générées.
-   **Documents :** Consultation de tous les documents partagés par le cabinet, téléchargement de documents, téléchargement de documents vers le cabinet (avec workflow de validation), recherche de documents.
-   **Communications :** Envoyer des messages au cabinet, consulter l'historique des échanges, demander des attestations ou des documents.
-   **Informations sur l'entreprise :** Consulter le dossier de l'entreprise, les coordonnées du manager assigné, demander la modification des informations (soumis à validation du cabinet).

#### Restrictions

-   Consultation uniquement (aucune capacité de modification des données).
-   Pas d'accès aux calculs détaillés des déclarations ou de la paie.
-   Aucune capacité de suppression.
-   Pas d'accès aux données des autres clients du cabinet.
-   Accès restreint aux documents partagés explicitement par le cabinet.

### 4.9. Tableaux de Bord et Rapports

Le système doit fournir des vues synthétiques de l'activité et des rapports détaillés pour les diverses parties prenantes, avec des visualisations dynamiques et configurables.

#### Tableau de Bord Manager/Gérant

-   **Indicateurs Clés :** Nombre total de clients actifs, nombre de déclarations pour le mois, déclarations en attente de validation, déclarations soumises, délais hebdomadaires/mensuels, documents en attente de validation, tâches assignées à l'équipe.
-   **Visualisations (via ECharts) :**

    Évolution des déclarations par mois : Représentation graphique (courbes ou barres) du nombre de déclarations par type (TVA, IS, CNSS) et par mois, pour suivre les tendances et la charge de travail. Utilisation de données réelles provenant du système.

    Distribution des clients par secteur d'activité : Diagramme circulaire ou en barres affichant la répartition des clients actifs par secteur, pour identifier les spécialisations du cabinet. Données clients du système.

    Performance de l'équipe (déclarations traitées) : Graphique en barres ou ligne montrant le nombre de déclarations validées ou soumises par assistant/manager sur une période donnée. Données issues des journaux d'audit et des workflows.

    Statut global des déclarations pour le mois : Diagramme de progression ou circulaire affichant le pourcentage de déclarations "À Compléter", "En Révision", "Validées", "Soumises". Données agrégées en temps réel.

-   **Alertes :** Déclarations en retard (rouge), délais à J-3 (orange), validations en attente depuis > 48h (jaune).

#### Tableau de Bord Assistant

-   **Liste des Tâches (To-Do List) :** Déclarations à compléter (par date limite), documents à traiter, tâches assignées.
-   **Mes Clients :** Liste des clients assignés, statut de leurs déclarations mensuelles.
-   **Tâches Assignées :** Liste des tâches avec statut (En cours, À faire, Terminé) et date limite.

#### Rapports Exportables

-   **Rapport d'Activité Mensuel :** Nombre de déclarations traitées par type, montants déclarés (TVA, IS, CNSS), nombre de fiches de paie générées, documents traités. Exportable au format PDF ou Excel.
-   **Rapport Annuel Client :** Toutes les déclarations de l'année, cumuls fiscaux et sociaux, coût total du personnel. Exportable au format PDF.
-   **Rapport des Délais :** Tous les délais à venir (30/60/90 jours), filtrable par client, type de déclaration. Exportable au format Excel.
-   **Livre de Paie :** Registre légal obligatoire pour tous les employés, tous les mois. Exportable au format PDF conforme.

### 4.10. Gestion des Utilisateurs et des Droits d'Accès

Le système doit contrôler l'accès en fonction des rôles et des responsabilités des utilisateurs, géré via le projet Admin pour les utilisateurs de la plateforme MetaCompta et le Back-Office pour les utilisateurs internes du cabinet et les utilisateurs Extranet clients. Le système implémentera une autorisation basée sur les permissions à l'aide d'ASP.NET Core Identity et de politiques dynamiques, comme illustré par les extraits de code fournis.

#### A. Création et Gestion des Utilisateurs

-   **Création :** Le SuperAdmin du cabinet (via le Back-Office) ou l'Admin MetaCompta (via le projet Admin) peut :
    -   Créer un utilisateur (nom, prénom, email, mot de passe temporaire généré et envoyé, statut actif/inactif).
    -   Assigner un profil prédéfini (SuperAdmin, Manager, Assistant, Client).
    -   Alternativement, créer un profil personnalisé avec des droits granulaires spécifiques.
    -   Associer un utilisateur à un ou plusieurs clients (pour les profils Manager/Assistant) ou à une entreprise cliente spécifique (pour les profils Client Extranet).
-   **Actions :** Activer/désactiver des utilisateurs, réinitialiser des mots de passe, modifier des informations utilisateur.
-   **Séparation claire :** Distinguer les utilisateurs internes du cabinet (qui ont accès au Back-Office) des utilisateurs des clients (qui ont accès à l'Extranet). Les employés du cabinet fiduciaire sont distincts des employés des entreprises clientes qui sont gérés via la section 4.4.

#### B. Profils et Permissions (Basés sur l'autorisation par permission dans .NET Core)

Le système utilise des profils, qui sont des regroupements de permissions spécifiques. La gestion est fortement inspirée de l';approche décrite par les extraits de code .NET Core, utilisant des attributs \`Authorize\` et des politiques dynamiques.

-   **SuperAdmin du Cabinet :** Accès complet au Back-Office du cabinet, gestion des utilisateurs du cabinet, gestion des profils, configuration des paramètres spécifiques au cabinet, consultation des journaux d'audit.
-   **Manager :** Gestion complète des clients (lecture/écriture), validation des déclarations, gestion des employés et de la paie, gestion documentaire, consultation de rapports. Pas de gestion des utilisateurs.
-   **Assistant :** Création/modification limitée de clients, saisie des déclarations (pas de validation finale), saisie des employés et de la paie, téléchargement de documents. Pas d'accès aux rapports stratégiques.
-   **Client (Portail) :** Consultation uniquement, téléchargement de documents partagés, téléchargement de documents vers le cabinet, messagerie. Accès limité à ses propres données d'entreprise.

Le mécanisme sous-jacent est basé sur l'ajout de "Claims" (revendications) de type "Permission" aux rôles, et l'utilisation de \`PermissionAuthorizationHandler\` et \`PermissionPolicyProvider\` pour évaluer ces permissions de manière dynamique.

#### C. Profils Personnalisés et Droits Granulaires

Le système permet de créer des profils personnalisés en sélectionnant parmi une liste de 60+ permissions, catégorisées par module. Ces permissions peuvent être gérées via une interface d'administration dédiée (accessible au SuperAdmin du cabinet).

-   **Permissions Client :** `Client.View`, `Client.Create`, `Client.Edit`, `Client.Delete`, `Client.Archive`, `Client.Configure`.
-   **Permissions Déclaration :** `Declaration.View`, `Declaration.Create`, `Declaration.Edit`, `Declaration.Validate`, `Declaration.Submit`, `Declaration.Delete`, `Declaration.Revert`.
-   **Permissions Employé (clients du cabinet) :** `Employee.View`, `Employee.Create`, `Employee.Edit`, `Employee.Delete`, `Employee.Archive`, `Employee.ManageAbsences`.
-   **Permissions Paie :** `Payroll.View`, `Payroll.Create`, `Payroll.Edit`, `Payroll.Validate`, `Payroll.Process`, `Payroll.GenerateSlips`, `Payroll.GenerateBankFile`.
-   **Permissions Document :** `Document.View`, `Document.Upload`, `Document.Edit`, `Document.Validate`, `Document.Delete`, `Document.Share`, `Document.Download`.
-   **Permissions Système (cabinet) :** `System.Admin` (pour le Gérant de cabinet, accès aux configurations générales), `User.Manage`, `Profile.Manage`, `Audit.View`, `Configuration.View`, `Configuration.Edit`, `Task.ManageAll`.

**Exemple :** Un profil "Assistant Junior" pourrait avoir uniquement `Client.View`, `Declaration.Create` et `Declaration.Edit` (mais pas `Validate` ou `Submit`), `Employee.View`, `Employee.Create`, et `Document.Upload`.

Le mécanisme de gestion des permissions doit permettre la création de permissions dynamiques, par exemple `Permissions.Module.Action`, et leur assignation aux rôles/profils.

#### D. Traçabilité (Audit Logs)

Chaque action significative doit être enregistrée de manière exhaustive :

-   **Qui :** L'utilisateur ayant effectué l'action.
-   **Quand :** Date et heure de l'action.
-   **Quoi :** Description de l'action (création, modification, suppression, validation, soumission, etc.).
-   **Sur quel élément :** L'identifiant de l'entité concernée (client, déclaration, document, employé).
-   **Depuis quelle adresse IP.**
-   **Ancienne/Nouvelle Valeur :** Pour les modifications, enregistrer les valeurs avant et après le changement.

Le SuperAdmin du cabinet peut consulter des journaux d'audit complets avec des capacités de recherche et de filtrage.

### 4.11. Gestion des Tâches

Le système doit permettre la création, l'assignation et le suivi des tâches pour les employés du cabinet, facilitant la collaboration et le respect des délais.

#### A. Création et Assignation de Tâches

-   Les Managers et les Gérants peuvent créer de nouvelles tâches.
-   **Chaque tâche doit inclure :**
    -   Titre de la tâche.
    -   Description détaillée.
    -   Client lié (optionnel).
    -   Déclaration liée (optionnel).
    -   Employé du cabinet assigné (un ou plusieurs).
    -   Date de début, Date d'échéance.
    -   Priorité (Faible, Moyenne, Haute, Urgente - référentiel configurable).
    -   Statut (À Faire, En Cours, En Attente de Révision, Terminé, Annulé - référentiel configurable).
    -   Pièces jointes (optionnel).
    -   Commentaires et historique des activités sur la tâche.

#### B. Suivi des Tâches

-   Les utilisateurs peuvent consulter leurs propres tâches via leur tableau de bord.
-   Les Managers et Gérants peuvent visualiser toutes les tâches de leur équipe, filtrer par employé, client, statut, ou date d'échéance.
-   Notifications automatiques lors de l'assignation d'une tâche, du changement de statut ou de l'approche de la date d'échéance.
-   Gantt Chart ou vue Kanban pour une visualisation graphique de l'avancement des tâches (optionnel pour phase ultérieure).

#### C. Règles Métier

-   Seuls les Managers et Gérants peuvent assigner des tâches.
-   Les utilisateurs assignés peuvent mettre à jour le statut et ajouter des commentaires.
-   La suppression de tâche est soumise à des permissions spécifiques.
-   Intégration avec le workflow de déclaration : une tâche peut être générée automatiquement (ex: "Valider Déclaration TVA") lors d'un changement de statut dans le workflow.

## 5\. Exigences Non Fonctionnelles

Ces exigences définissent les attributs de qualité du système, au-delà de son périmètre fonctionnel.

### 5.1. Performance

-   **Temps de Chargement :** Les pages principales doivent se charger en moins de 2 secondes.
-   **Téléchargement de Documents :** Prise en charge des fichiers jusqu'à 50 Mo avec des temps de téléchargement raisonnables (<30 secondes pour 10 documents).
-   **Évolutivité :** Prise en charge de 100 cabinets simultanés, chacun avec 13 utilisateurs, sans dégradation significative des performances.
-   **Calculs :** La génération d'une fiche de paie pour un seul employé doit prendre moins de 1 seconde.
-   **Recherche :** Les résultats de recherche doivent être affichés en moins de 1 seconde pour une base de données de 10 000 documents.

### 5.2. Sécurité

-   **Authentification :** Politique de mot de passe fort obligatoire (8+ caractères, majuscule, chiffre, symbole).
-   **Sessions :** Déconnexion automatique après 30 minutes d'inactivité (configurable).
-   **Chiffrement des Données :** Les données sensibles (salaires, CIN, RIB) doivent être chiffrées au repos et en transit.
-   **Connexion :** HTTPS (TLS 1.3) doit être appliqué pour toutes les communications.
-   **Sauvegardes :** Sauvegardes automatiques quotidiennes, conservées pendant 30 jours.
-   **Audit :** Traçabilité complète de toutes les actions sensibles (voir 4.10.D).
-   **Gestion des Vulnérabilités :** Audits de sécurité réguliers et tests d'intrusion.

### 5.3. Disponibilité

-   **Temps de Fonctionnement (Uptime) :** Minimum 99.5% (environ 3.65 jours d'indisponibilité maximum par an).
-   **Fenêtre de Maintenance :** Fenêtre de maintenance hebdomadaire (par exemple, dimanche 2h00 - 6h00 GMT+1).
-   **Support :** Assistance disponible du lundi au vendredi, de 9h00 à 18h00 GMT+1.
-   **Temps de Réponse :** Moins de 24 heures pour le support standard, moins de 4 heures pour le support premium.

### 5.4. Ergonomie et Localisation Multilingue

-   **Interface :** Conception simple, intuitive et moderne.
-   **Réactivité (Responsiveness) :** Accessible et entièrement fonctionnel sur les ordinateurs de bureau, les tablettes et les smartphones.
-   **Langues :** Français comme langue principale de l'interface, avec support complet de l'Arabe (`ar-MA`) et de l'Anglais (`en-US`) via la localisation .NET Core.
    -   **Implémentation :** Utilisation de fichiers de ressources (`.resx`) pour la globalisation et la localisation des chaînes de l'interface utilisateur, des messages d'erreur et des libellés.
    -   **Gestion des Traductions :** Un espace dédié dans le projet Admin (pour les administrateurs MetaCompta) ou dans le Back-Office (pour les SuperAdmins de cabinet pour les textes spécifiques du cabinet) permettra d'éditer les textes de traduction directement via une interface utilisateur, sans nécessiter de recompilation du code. Cela implique la persistance des traductions en base de données pour une flexibilité maximale.
    -   **Formatage :** Le système gérera automatiquement le formatage des dates, des nombres et des devises en fonction de la culture sélectionnée.
-   **Assistance Utilisateur :** Infobulles, aide contextuelle et documentation en ligne.
-   **Accessibilité :** Conformité aux normes WCAG 2.1 niveau AA.

### 5.5. Compatibilité

-   **Navigateurs :**
    -   Chrome 90+
    -   Firefox 88+
    -   Edge 90+
    -   Safari 14+
-   **Systèmes d'Exploitation (Côté Client) :**
    -   Windows 10/11
    -   macOS 11+
    -   Linux (Ubuntu, Debian)
    -   iOS 14+, Android 10+

### 5.6. Conformité Légale

-   **Législation Marocaine :** Conformité totale avec le Code Général des Impôts (CGI), le Code du Travail, la réglementation de la CNSS et la Loi 09-08 sur la protection des données personnelles.
-   **Fiches de Paie :** Le format doit être conforme au modèle officiel marocain.
-   **Déclarations :** Toutes les déclarations générées doivent être conformes aux formulaires de la DGI (SIMPL) et de la CNSS (DAMANCOM).
-   **Rétention des Données :** Les documents et les données doivent être conservés pendant un minimum de 10 ans, conformément aux obligations légales marocaines.

## 6\. Cas d'Utilisation

Ces scénarios illustrent les interactions typiques avec le système MetaCompta, démontrant ses fonctionnalités du point de vue de différents utilisateurs.

### 6.1. Cas d'Utilisation: Traiter une Déclaration TVA Mensuelle

-   **Acteur :** Assistant Comptable
-   **Contexte :** Nous sommes le 20 mars 2026, et l'assistant doit compléter la déclaration TVA de février 2026 pour le client "SARL TechSolutions".
-   **Étapes du Scénario :**
    1.  **Connexion :** L'assistant se connecte et arrive sur son tableau de bord.
    2.  **Accès aux Déclarations :** Navigue vers "Déclarations", filtre par "TVA" et "À Compléter". Localise "SARL TechSolutions - TVA Février 2026" avec le statut "À Compléter" et la date limite "31 mars 2026".
    3.  **Saisie des Données :** Clique sur "Compléter". Sur le formulaire :
        -   Saisit le CA HT 20% : 250 000 DH (TVA 50 000 DH calculée automatiquement par le système, basé sur le taux TVA 20% du référentiel).
        -   Saisit les ventes exonérées : 15 000 DH.
        -   Saisit la TVA récupérable sur charges : 18 500 DH.
        -   Le crédit du mois précédent est de 2 500 DH (pré-rempli automatiquement par le système).
    4.  **Vérification des Calculs :** Le système affiche : TVA Collectée : 50 000 DH ; TVA Récupérable : 18 500 DH ; Crédit Précédent : 2 500 DH ; TVA NETTE À PAYER : 29 000 DH.
    5.  **Sauvegarde :** Clique sur "Sauvegarder". Le système confirme "Déclaration enregistrée avec succès". Le statut de la déclaration passe à "Complétée".
    6.  **Notification au Manager :** Le système notifie automatiquement le manager responsable : "Déclaration TVA Février 2026 - SARL TechSolutions prête pour validation".
-   **Résultat :** Déclaration complétée efficacement, sans erreurs de calcul, avec une traçabilité complète, et en attente de validation du manager.

### 6.2. Cas d'Utilisation: Valider et Soumettre une Déclaration

-   **Acteur :** Manager
-   **Contexte :** Le manager a reçu une notification indiquant qu'une déclaration TVA est prête à être validée.
-   **Étapes du Scénario :**
    1.  **Notification :** Le manager voit une notification ("1"), clique dessus et accède à la "Déclaration TVA Février 2026 - SARL TechSolutions prête pour validation".
    2.  **Examen :** Accède directement à la déclaration. Examine les montants saisis par rapport aux pièces justificatives (factures) disponibles dans le système.
    3.  **Validation :** Clique sur "Valider". Confirme l'action. Le statut de la déclaration passe à "Validée". Un PDF de la déclaration est automatiquement généré.
    4.  **Télé-déclaration Externe :** Le manager se connecte au portail SIMPL DGI, saisit les montants du PDF de MetaCompta et soumet la déclaration.
    5.  **Marquer comme Soumise :** Retourne à MetaCompta, clique sur "Marquer comme Soumise", saisit la date de soumission (par exemple, 25 mars 2026) et confirme. Le statut de la déclaration passe à "Soumise".
    6.  **Archivage :** Le système archive automatiquement le PDF de la déclaration, les documents justificatifs associés et les journaux de validation/soumission.
-   **Résultat :** Déclaration validée et soumise à temps, avec l'historique complet conservé, et le client automatiquement informé (si les notifications sont activées).

### 6.3. Cas d'Utilisation: Préparer la Paie Mensuelle

-   **Acteur :** Assistant Comptable
-   **Contexte :** Début mars 2026, l';assistant doit préparer la paie de février 2026 pour "SARL Distribution Nord", qui compte 15 employés.
-   **Étapes du Scénario :**
    1.  **Accès au Module Paie :** Navigue vers "Paie" > "Nouvelle Paie". Sélectionne "SARL Distribution Nord" et "Février 2026".
    2.  **Initialisation :** Le système charge les 15 employés actifs, pré-remplit les salaires de base et les primes fixes (basés sur les fiches employés). Affiche un tableau récapitulatif.
    3.  **Saisie des Variables :** Pour chaque employé, saisit les variables mensuelles (par exemple, Heures Supplémentaires 25% pour ALAMI Hassan : 10 heures, Prime Exceptionnelle : 1 000 DH ; Absence pour BENNANI Fatima : 2 jours).
    4.  **Calculs Automatiques :** Clique sur "Calculer Tout". Le système calcule instantanément le salaire brut, la CNSS, l'AMO, l'IR et le net à payer pour tous les employés, en utilisant les taux et plafonds configurés dynamiquement.
    5.  **Vérification :** L'assistant examine visuellement les 15 fiches de paie générées.
    6.  **Soumission pour Validation :** Clique sur "Soumettre pour Validation". Le statut de la paie passe à "En Attente de Validation". Le manager reçoit une notification.
    7.  **Validation du Manager :** Le manager examine la paie globale et quelques fiches individuelles, puis valide. Le statut passe à "Validée".
    8.  **Génération des Documents :** Le système génère automatiquement : 15 fiches de paie PDF, le Livre de Paie, le Journal Comptable, le Fichier de Virement Bancaire (CSV), les états pré-remplis CNSS et IR Salaires.
    9.  **Distribution :** Les fiches de paie sont disponibles dans l'espace employé (portail client). Le manager envoie le fichier de virement bancaire. Les documents sont archivés.
-   **Résultat :** La paie de 15 employés est traitée efficacement et avec précision, tous les documents requis sont générés automatiquement, assurant la conformité légale.

### 6.4. Cas d'Utilisation: Le Client Consulte Ses Déclarations

-   **Acteur :** Client Final (Manager de l'entreprise cliente)
-   **Contexte :** Le manager de "SARL TechSolutions" souhaite consulter ses déclarations fiscales.
-   **Étapes du Scénario :**
    1.  **Connexion au Portail :** Navigue vers [https://portail.metacompta\_1034.ma](https://portail.metacompta.ma), entre ses identifiants et accède à son tableau de bord client.
    2.  **Vue du Tableau de Bord :** Voit un résumé de l'entreprise et les délais à venir (par exemple, TVA Mars 2026 : 30 avril ; CNSS Mars 2026 : 10 avril).
    3.  **Consultation des Déclarations :** Clique sur "Mes Déclarations". Voit une liste complète. Filtre par "TVA".
    4.  **Détails de la Déclaration :** Clique sur "TVA Février 2026". Affiche le résumé : Période, Type, Statut (Soumise), Date de Soumission, Montant (29 000 DH).
    5.  **Téléchargement :** Clique sur "Télécharger PDF". Le PDF complet de la déclaration s'ouvre. Peut imprimer ou enregistrer.
    6.  **Consultation des Fiches de Paie :** Va dans "Employés & Paie". Voit la liste des employés. Clique sur "ALAMI Hassan". Télécharge sa fiche de paie de Février 2026.
-   **Résultat :** Le client accède de manière autonome à ses données, éliminant le besoin de contacter le cabinet pour des documents simples, améliorant ainsi la transparence et la satisfaction client.

## 7\. Planification et Phases

Le projet sera exécuté selon une approche progressive pour apporter une valeur incrémentale et gérer la complexité.

-   **Phase 1: Modules Coeurs (3 mois)**
    -   **Objectif :** Lancer un Produit Minimum Viable (MVP) fonctionnel.
    -   **Livrables :** Gestion des clients, déclarations TVA, gestion documentaire basique, 3 profils utilisateurs (SuperAdmin, Manager, Assistant), tableau de bord simple, gestion des tâches basique.
    -   **Critères de Succès :** 5 cabinets pilotes peuvent gérer leurs clients et leurs déclarations TVA.
-   **Phase 2: Déclarations Complètes (2 mois)**
    -   **Livrables :** Déclarations IS et IR, déclarations CNSS, notifications et alertes, tableaux de bord améliorés, workflows de déclaration configurables.
    -   **Critères de Succès :** Cycle complet des déclarations fiscales et sociales opérationnel.
-   **Phase 3: Paie et RH (2 mois)**
    -   **Livrables :** Gestion complète des employés, module de paie avec calculs automatiques, génération de fiches de paie PDF, registre de paie et journaux comptables.
    -   **Critères de Succès :** 3 cabinets pilotes utilisent le module de paie en production.
-   **Phase 4: Portail Client (1 mois)**
    -   **Livrables :** Portail client pour consultation, téléchargement de documents, messagerie cabinet-client.
    -   **Critères de Succès :** 50 entreprises clientes connectées et actives sur le portail.
-   **Phase 5: Optimisations et Localisation (1 mois)**
    -   **Livrables :** Rapports et exports avancés, profils personnalisés, optimisations des performances, corrections de bugs, implémentation complète de la localisation Arabe/Anglais et interface d'édition des traductions.
    -   **Critères de Succès :** Satisfaction utilisateur > 80%, stabilité du système, fonctionnalités multilingues opérationnelles.
-   **Phase 6: Lancement Commercial (Continu)**
    -   **Objectif :** Commercialisation et amélioration continue basée sur les retours du marché.

## 8\. Critères d'Acceptation

Les critères suivants doivent être remplis pour que le système MetaCompta soit considéré comme acceptable pour le déploiement.

-   **Gestion des Clients :**
    -   ✅ Créer un client avec toutes les informations requises en < 2 minutes.
    -   ✅ Rechercher un client par nom en < 1 seconde.
    -   ✅ Modifier un client avec une traçabilité complète.
-   **Déclarations :**
    -   ✅ Créer une déclaration TVA en < 5 minutes.
    -   ✅ Calculs automatiques précis à 100%.
    -   ✅ Validation et soumission en 3 clics.
    -   ✅ Générer un PDF conforme.
    -   ✅ Workflows de déclaration configurables et fonctionnels.
-   **Paie :**
    -   ✅ Préparer la paie pour 20 employés en < 1 heure.
    -   ✅ Calculs IR conformes au barème officiel.
    -   ✅ Générer des fiches de paie PDF conformes.
    -   ✅ Exporter un fichier de virement bancaire compatible.
-   **Documents :**
    -   ✅ Télécharger 10 documents en < 30 secondes.
    -   ✅ Rechercher un document en < 2 secondes.
    -   ✅ Téléchargement instantané.
-   **Portail Client :**
    -   ✅ Le client accède à ses données en < 3 clics.
    -   ✅ L'interface est compréhensible sans formation.
    -   ✅ Télécharger des fiches de paie sans assistance.
-   **Performance :**
    -   ✅ Les pages se chargent en < 2 secondes.
    -   ✅ Le système prend en charge 100 utilisateurs simultanés.
    -   ✅ Uptime > 99%.
-   **Localisation :**
    -   ✅ L'interface est entièrement traduisible en Français, Arabe, Anglais.
    -   ✅ L'outil d'édition des traductions est fonctionnel.
-   **Droits d'Accès :**
    -   ✅ Les permissions granulaires sont correctement appliquées et empêchent les accès non autorisés.
    -   ✅ Les journaux d'audit enregistrent toutes les actions pertinentes.

## 9\. Glossaire

Terme

Définition

Cabinet Fiduciaire

Une entreprise fournissant des services comptables, fiscaux et sociaux à d'autres entreprises.

Client Final

L'entreprise cliente du cabinet fiduciaire.

TVA

Taxe sur la Valeur Ajoutée - un impôt indirect sur la consommation.

IS

Impôt sur les Sociétés - impôt sur les bénéfices des entreprises.

IR

Impôt sur le Revenu - impôt sur le revenu des particuliers.

CNSS

Caisse Nationale de Sécurité Sociale - l'organisme national de sécurité sociale au Maroc.

AMO

Assurance Maladie Obligatoire.

ICE

Identifiant Commun de l'Entreprise - identifiant unique pour les entreprises au Maroc.

IF

Identifiant Fiscal.

RC

Registre de Commerce.

SIMPL

Système d'Information de la Direction Générale des Impôts - le portail de télé-déclaration de la DGI.

DAMANCOM

La plateforme de télé-déclaration de la CNSS.

Barème Progressif

Système de barème d'imposition progressif basé sur les tranches de revenus.

Bulletin de Paie

Fiche de paie - document mensuel détaillant la rémunération de l'employé.

Livre de Paie

Registre de paie - registre légal obligatoire récapitulant toutes les paies.

Dossier Client

Terme désignant un ensemble de fichiers ou d'informations relatives à un client.

Référentiel Dynamique

Ensemble de données (taux, plafonds, listes, etc.) configurables via l'interface utilisateur et stockées en base de données pour assurer la flexibilité du système.

## 10\. Annexes

Informations supplémentaires et exemples pour appuyer les exigences.

### 10.1. Annexe A: Exemples de Calculs

#### Exemple: Calcul TVA

-   Client: Restaurant "Le Gourmet"
-   Période: Mars 2026
-   Chiffre d'Affaires HT 20%: 150 000 DH
-   TVA Collectée 20%: 30 000 DH
-   Achats HT: 80 000 DH
-   TVA Récupérable: 16 000 DH
-   Crédit TVA Février: 0 DH
-   **Calcul :** TVA Nette = 30 000 - 16 000 - 0 = 14 000 DH à payer.

#### Exemple: Calcul IR Salaire

-   Employé: ALAMI Hassan
-   Salaire Brut Mensuel: 10 000 DH
-   Salaire Brut Annuel: 120 000 DH
-   Cotisations Sociales Annuelles (CNSS+AMO+CIMR fictif): 9 600 DH
-   Revenu Net Imposable: 110 400 DH
-   Déductions pour Charges de Famille (4 enfants × 360 DH/enfant) : 1 440 DH
-   Revenu Imposable Net après déductions: 108 960 DH
-   **Application du barème progressif (référencé au 2026-03-15) :**
    -   30 000 DH × 0% = 0
    -   20 000 DH × 10% = 2 000
    -   10 000 DH × 20% = 2 000
    -   20 000 DH × 30% = 6 000
    -   28 960 DH × 34% = 9 846,4
-   IR Annuel Total = 19 846,4 DH
-   IR Mensuel = 19 846,4 / 12 = 1 653,87 DH

### 10.2. Annexe B: Taux et Plafonds Applicables (2026) & Référentiels Dynamiques

Tous les taux, plafonds et barèmes listés ci-dessous sont à titre indicatif et doivent être implémentés comme des **référentiels dynamiques configurables et stockés en base de données** via le module Admin ou par le Gérant de cabinet, pour s'adapter aux évolutions réglementaires sans nécessiter de modifications de code.

-   **TVA (Taxes sur la Valeur Ajoutée) :**
    -   Taux Normal : 20%
    -   Taux Réduits : 14%, 10%, 7%
    -   Exonérations : Produits de première nécessité, médicaments, etc. (liste configurable).
-   **CNSS (Caisse Nationale de Sécurité Sociale) :**
    -   Plafond de cotisation : 6 000 DH/mois (configurable).
    -   Cotisation Salariale : 6.74% (configurable).
    -   Cotisation Patronale : 20.48% (configurable).
-   **AMO (Assurance Maladie Obligatoire) :**
    -   Cotisation Salariale : 2.26% (configurable).
    -   Cotisation Patronale : 4.11% (configurable).
-   **Formation Professionnelle :**
    -   Taux Employeur : 1.6% (configurable).
-   **IS (Impôt sur les Sociétés) :**
    -   Taux Normal : 31% (configurable).
    -   Taux Réduit : 20% (pour les PME, configurable).
    -   Taux Minimum : 10% (certains secteurs, configurable).
    -   Barèmes et seuils pour les taux réduits et minimums (configurables).
-   **IR (Impôt sur le Revenu) :**
    -   Barème Progressif (voir section 4.2.3, tous les tranches et taux sont configurables).
    -   Montant des déductions pour charges de famille (ex: 360 DH/enfant, plafond 6 enfants - tous configurables).
    -   Autres abattements et déductions légales (configurables).
-   **Périodes de Déclaration :**
    -   Types (Mensuel, Trimestriel, Annuel) configurables.
    -   Dates limites par défaut pour chaque type de déclaration (configurables).
-   **Catégories de Documents :**
    -   Liste des catégories de documents (voir 4.6.B) entièrement configurable.
-   **Statuts de Workflow :**
    -   Statuts des déclarations et des documents (ex: Brouillon, Validé, Soumis) configurables.
    -   Transitions autorisées entre statuts (configurables et liées aux permissions).
-   **Types de Contrat Employé / Motifs de Départ :**
    -   Listes déroulantes pour la gestion des employés (CDI, CDD, Démission, Licenciement, etc.) entièrement configurables.

// Sample ECharts integration for conceptual visualizations document.addEventListener('DOMContentLoaded', function () { // Chart: Evolution des déclarations par mois var chart1 = echarts.init(document.getElementById('chart\_declarations\_evolution')); var option1 = { title: { text: 'Évolution des Déclarations par Mois (Conceptuel)' }, tooltip: { trigger: 'axis' }, legend: { data: \['TVA', 'IS', 'CNSS'\] }, xAxis: { type: 'category', data: \['Jan', 'Fév', 'Mar', 'Avr', 'Mai', 'Juin'\] }, yAxis: { type: 'value' }, series: \[ { name: 'TVA', type: 'line', data: \[120, 132, 101, 134, 90, 230\] }, { name: 'IS', type: 'line', data: \[20, 12, 15, 30, 25, 28\] }, { name: 'CNSS', type: 'line', data: \[150, 160, 140, 170, 155, 180\] } \] }; chart1.setOption(option1); // Chart: Distribution des clients par secteur d'activité var chart2 = echarts.init(document.getElementById('chart\_client\_distribution')); var option2 = { title: { text: 'Distribution des Clients par Secteur (Conceptuel)', left: 'center' }, tooltip: { trigger: 'item' }, legend: { orient: 'vertical', left: 'left', data: \['Services', 'Commerce', 'Industrie', 'BTP', 'Autres'\] }, series: \[ { name: 'Secteurs', type: 'pie', radius: '55%', center: \['50%', '60%'\], data: \[ { value: 335, name: 'Services' }, { value: 310, name: 'Commerce' }, { value: 234, name: 'Industrie' }, { value: 135, name: 'BTP' }, { value: 154, name: 'Autres' } \], emphasis: { itemStyle: { shadowBlur: 10, shadowOffsetX: 0, shadowColor: 'rgba(0, 0, 0, 0.5)' } } } \] }; chart2.setOption(option2); // Chart: Performance de l'équipe (déclarations traitées) var chart3 = echarts.init(document.getElementById('chart\_team\_performance')); var option3 = { title: { text: 'Performance de l\\'Équipe (Déclarations Traitées - Conceptuel)' }, tooltip: { trigger: 'axis' }, xAxis: { type: 'category', data: \['Assistant A', 'Assistant B', 'Manager C', 'Manager D'\] }, yAxis: { type: 'value' }, series: \[{ name: 'Déclarations', type: 'bar', data: \[50, 45, 30, 35\] }\] }; chart3.setOption(option3); // Chart: Statut global des déclarations pour le mois var chart4 = echarts.init(document.getElementById('chart\_global\_declaration\_status')); var option4 = { title: { text: 'Statut Global des Déclarations (Conceptuel)', left: 'center' }, tooltip: { trigger: 'item' }, legend: { orient: 'vertical', left: 'left', data: \['À Compléter', 'En Révision', 'Validées', 'Soumises'\] }, series: \[ { name: 'Statut', type: 'pie', radius: \['40%', '70%'\], avoidLabelOverlap: false, label: { show: false, position: 'center' }, emphasis: { label: { show: true, fontSize: '20', fontWeight: 'bold' } }, labelLine: { show: false }, data: \[ { value: 10, name: 'À Compléter' }, { value: 25, name: 'En Révision' }, { value: 40, name: 'Validées' }, { value: 25, name: 'Soumises' } \] } \] }; chart4.setOption(option4); // Resize charts on window resize window.addEventListener('resize', function () { chart1.resize(); chart2.resize(); chart3.resize(); chart4.resize(); }); });

© 2026 MetaCompta. Tous droits réservés.