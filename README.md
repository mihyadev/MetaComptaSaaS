# 🏛️ MetaCompta SAS

**Plateforme de Gestion Intégrée pour Cabinets Fiduciaires Marocains**

---

## 📋 Description

MetaCompta est une solution SaaS complète conçue pour les cabinets fiduciaires marocains. Elle offre :

- **Gestion des Clients** : CRUD complet avec identifiants marocains (ICE, IF, RC, CNSS)
- **Déclarations Fiscales** : TVA (20%, 14%, 10%, 7%), IS, CNSS automatisées
- **Gestion de la Paie** : Calcul automatique CNSS, AMO, IR selon le barème progressif marocain
- **Portail Client (Extranet)** : Accès sécurisé pour les clients du cabinet
- **Administration SaaS** : Gestion des cabinets, abonnements, données de référence

## 🏗️ Architecture

```
MetaComptaSAS/
├── src/
│   ├── MetaCompta.Domain/          # Entités, modèles de domaine
│   ├── MetaCompta.Infrastructure/  # EF Core, DbContext, migrations
│   ├── MetaCompta.Application/     # Services métier, DTOs, interfaces
│   ├── MetaCompta.API/             # API REST (JWT + Swagger)
│   ├── MetaCompta.BackOffice/      # Interface de gestion (MVC + Bootstrap 5)
│   ├── MetaCompta.Extranet/        # Portail client
│   └── MetaCompta.Admin/           # Administration plateforme
├── Dockerfile
├── docker-compose.yml
└── MetaComptaSAS.sln
```

## 🛠️ Technologies

| Couche | Technologies |
|--------|-------------|
| **Backend** | .NET 9, ASP.NET Core 9, EF Core 9 |
| **Base de données** | SQL Server 2022 |
| **Authentification** | ASP.NET Identity + JWT Bearer |
| **Frontend** | Bootstrap 5.3.3, jQuery 3.7.1, ECharts 5.5, DataTables |
| **Conteneurisation** | Docker, Docker Compose |

## 🚀 Démarrage Rapide

### Prérequis
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server) (ou Docker)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optionnel)

### Option 1 : Développement Local

```bash
# Restaurer les packages
dotnet restore

# Appliquer les migrations (la DB sera créée automatiquement au démarrage)
# Modifier la connection string dans src/MetaCompta.API/appsettings.json

# Lancer l'API
cd src/MetaCompta.API
dotnet run

# Lancer le BackOffice (dans un autre terminal)
cd src/MetaCompta.BackOffice
dotnet run
```

### Option 2 : Docker Compose

```bash
# Build et lancement de tous les services
docker-compose up --build -d

# Vérifier les logs
docker-compose logs -f
```

## 🌐 URLs

| Service | URL | Description |
|---------|-----|-------------|
| **API** | `https://localhost:7100` | API REST + Swagger UI |
| **BackOffice** | `http://localhost:7200` | Interface de gestion |
| **Extranet** | `http://localhost:7300` | Portail client |
| **Admin** | `http://localhost:7400` | Administration SaaS |

## 🔑 Comptes de Démonstration

| Rôle | Email | Mot de passe |
|------|-------|-------------|
| **Admin Plateforme** | `admin@metacompta.ma` | `Admin@2026!` |
| **Gérant Cabinet** | `gerant@cabinet-demo.ma` | `Gerant@2026!` |

## 📊 Fonctionnalités Fiscales Marocaines

### TVA
- Taux : 20%, 14%, 10%, 7%, 0% (exonéré)
- Gestion du crédit de TVA reporté
- Calcul automatique TVA due / crédit à reporter

### CNSS & AMO
- CNSS Salariale : 4.48% (plafond 6 000 MAD)
- CNSS Patronale : 8.98% (plafond 6 000 MAD)
- AMO Salariale : 2.26% (sans plafond)
- AMO Patronale : 4.11% (sans plafond)

### IR (Impôt sur le Revenu)
Barème progressif annuel :
| Tranche | Taux | Somme à déduire |
|---------|------|-----------------|
| 0 - 30 000 | 0% | 0 |
| 30 001 - 50 000 | 10% | 3 000 |
| 50 001 - 60 000 | 20% | 8 000 |
| 60 001 - 80 000 | 30% | 14 000 |
| 80 001 - 180 000 | 34% | 17 200 |
| > 180 000 | 38% | 24 400 |

- Déduction pour charges de famille : 360 MAD/personne (max 6)
- Frais professionnels : 20% du brut imposable (plafond 30 000 MAD/an)

## 📄 Licence

© 2026 MetaCompta SAS. Tous droits réservés.
