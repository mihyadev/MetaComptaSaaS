using MetaCompta.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MetaCompta.BackOffice.Services;

public class BulletinPdfService
{
    static BulletinPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(BulletinCompletDto b)
    {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(30);
                page.MarginVertical(25);
                page.DefaultTextStyle(x => x.FontSize(8.5f).FontFamily("Helvetica"));

                page.Header().Element(c => ComposeHeader(c, b));
                page.Content().Element(c => ComposeContent(c, b));
                page.Footer().Element(c => ComposeFooter(c, b));
            });
        });

        return doc.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, BulletinCompletDto b)
    {
        container.Column(col =>
        {
            // Title bar
            col.Item().Background("#1a3a5c").Padding(10).Row(row =>
            {
                row.RelativeItem().Text("BULLETIN DE PAIE").FontSize(16).Bold().FontColor("#ffffff");
                row.ConstantItem(140).AlignRight().Text(b.PeriodeLabel ?? $"{b.Mois:D2}/{b.Annee}").FontSize(12).Bold().FontColor("#ffffff");
            });

            col.Item().Height(8);

            // Employer + Employee boxes
            col.Item().Row(row =>
            {
                // Employeur
                row.RelativeItem().Border(0.5f).BorderColor("#cccccc").Padding(8).Column(c =>
                {
                    c.Item().Text("EMPLOYEUR").FontSize(7).Bold().FontColor("#1a3a5c");
                    c.Item().Height(4);
                    c.Item().Text(b.EmployeurRaisonSociale ?? "—").FontSize(10).Bold();
                    if (!string.IsNullOrEmpty(b.EmployeurAdresse))
                        c.Item().Text(b.EmployeurAdresse).FontSize(7.5f).FontColor("#555555");
                    c.Item().Height(3);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text(t => { t.Span("ICE: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeurICE ?? "—").FontSize(7.5f); });
                        r.RelativeItem().Text(t => { t.Span("IF: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeurIF ?? "—").FontSize(7.5f); });
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text(t => { t.Span("CNSS: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeurCNSS ?? "—").FontSize(7.5f); });
                    });
                });

                row.ConstantItem(10);

                // Employe
                row.RelativeItem().Border(0.5f).BorderColor("#cccccc").Padding(8).Column(c =>
                {
                    c.Item().Text("EMPLOYE").FontSize(7).Bold().FontColor("#1a3a5c");
                    c.Item().Height(4);
                    c.Item().Text(b.EmployeeNom ?? "—").FontSize(10).Bold();
                    c.Item().Height(3);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text(t => { t.Span("CIN: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeeCIN ?? "—").FontSize(7.5f); });
                        r.RelativeItem().Text(t => { t.Span("Mat: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeeMatricule ?? "—").FontSize(7.5f); });
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text(t => { t.Span("CNSS: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeeCNSS ?? "—").FontSize(7.5f); });
                        r.RelativeItem().Text(t => { t.Span("Poste: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeePoste ?? "—").FontSize(7.5f); });
                    });
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text(t => { t.Span("Embauche: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeeDateEmbauche?.ToString("dd/MM/yyyy") ?? "—").FontSize(7.5f); });
                        r.RelativeItem().Text(t => { t.Span("Sit. Fam.: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeeSituationFamiliale ?? "—").FontSize(7.5f); });
                    });
                    if (b.EmployeeNbEnfants > 0)
                        c.Item().Text(t => { t.Span("Enfants: ").FontSize(7).FontColor("#888888"); t.Span(b.EmployeeNbEnfants.ToString()).FontSize(7.5f); });
                });
            });

            col.Item().Height(8);
        });
    }

    private void ComposeContent(IContainer container, BulletinCompletDto b)
    {
        container.Column(col =>
        {
            // Section table for each bulletin section
            foreach (var section in b.Sections)
            {
                col.Item().Background("#e8edf2").Padding(5).Text(section.SectionLabel.ToUpper()).FontSize(8).Bold().FontColor("#1a3a5c");

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cd =>
                    {
                        cd.ConstantColumn(40);    // Code
                        cd.RelativeColumn(3);     // Libelle
                        cd.ConstantColumn(75);    // Base
                        cd.ConstantColumn(55);    // Taux
                        cd.ConstantColumn(75);    // Gain
                        cd.ConstantColumn(75);    // Retenue
                    });

                    // Header
                    table.Header(h =>
                    {
                        h.Cell().BorderBottom(0.5f).BorderColor("#cccccc").Padding(3).Text("Code").FontSize(7).Bold().FontColor("#888888");
                        h.Cell().BorderBottom(0.5f).BorderColor("#cccccc").Padding(3).Text("Designation").FontSize(7).Bold().FontColor("#888888");
                        h.Cell().BorderBottom(0.5f).BorderColor("#cccccc").Padding(3).AlignRight().Text("Base").FontSize(7).Bold().FontColor("#888888");
                        h.Cell().BorderBottom(0.5f).BorderColor("#cccccc").Padding(3).AlignRight().Text("Taux").FontSize(7).Bold().FontColor("#888888");
                        h.Cell().BorderBottom(0.5f).BorderColor("#cccccc").Padding(3).AlignRight().Text("Gain").FontSize(7).Bold().FontColor("#888888");
                        h.Cell().BorderBottom(0.5f).BorderColor("#cccccc").Padding(3).AlignRight().Text("Retenue").FontSize(7).Bold().FontColor("#888888");
                    });

                    foreach (var ligne in section.Lignes.OrderBy(l => l.OrdreAffichage))
                    {
                        table.Cell().BorderBottom(0.3f).BorderColor("#eeeeee").Padding(3).Text(ligne.RubriqueCode).FontSize(7).FontColor("#999999");
                        table.Cell().BorderBottom(0.3f).BorderColor("#eeeeee").Padding(3).Text(ligne.Libelle).FontSize(8);
                        table.Cell().BorderBottom(0.3f).BorderColor("#eeeeee").Padding(3).AlignRight().Text(ligne.Base != 0 ? ligne.Base.ToString("N2") : "").FontSize(8);
                        table.Cell().BorderBottom(0.3f).BorderColor("#eeeeee").Padding(3).AlignRight().Text(ligne.Taux != 0 ? ligne.Taux.ToString("N4") : "").FontSize(8);
                        table.Cell().BorderBottom(0.3f).BorderColor("#eeeeee").Padding(3).AlignRight().Text(ligne.Gain != 0 ? ligne.Gain.ToString("N2") : "").FontSize(8).FontColor("#2e7d32");
                        table.Cell().BorderBottom(0.3f).BorderColor("#eeeeee").Padding(3).AlignRight().Text(ligne.Retenue != 0 ? ligne.Retenue.ToString("N2") : "").FontSize(8).FontColor("#c62828");
                    }

                    // Section totals
                    if (section.TotalGains != 0 || section.TotalRetenues != 0)
                    {
                        table.Cell().ColumnSpan(4).BorderTop(0.5f).BorderColor("#cccccc").Padding(3).AlignRight().Text($"Sous-total {section.SectionLabel}").FontSize(7.5f).Bold();
                        table.Cell().BorderTop(0.5f).BorderColor("#cccccc").Padding(3).AlignRight().Text(section.TotalGains != 0 ? section.TotalGains.ToString("N2") : "").FontSize(8).Bold().FontColor("#2e7d32");
                        table.Cell().BorderTop(0.5f).BorderColor("#cccccc").Padding(3).AlignRight().Text(section.TotalRetenues != 0 ? section.TotalRetenues.ToString("N2") : "").FontSize(8).Bold().FontColor("#c62828");
                    }
                });

                col.Item().Height(4);
            }

            // ═══ TOTAUX ═══
            col.Item().Height(8);
            col.Item().Background("#1a3a5c").Padding(1).Column(_ => { }); // separator line

            col.Item().Padding(6).Table(table =>
            {
                table.ColumnsDefinition(cd =>
                {
                    cd.RelativeColumn(2);
                    cd.RelativeColumn(1);
                });

                void AddTotalRow(string label, decimal value, string color, bool bold = false, float fontSize = 9f)
                {
                    if (bold)
                    {
                        table.Cell().Padding(2).Text(label).FontSize(fontSize).Bold();
                        table.Cell().Padding(2).AlignRight().Text($"{value:N2} MAD").FontSize(fontSize).Bold().FontColor(color);
                    }
                    else
                    {
                        table.Cell().Padding(2).Text(label).FontSize(fontSize);
                        table.Cell().Padding(2).AlignRight().Text($"{value:N2} MAD").FontSize(fontSize).FontColor(color);
                    }
                }

                AddTotalRow("TOTAL DES GAINS", b.TotalGains, "#2e7d32");
                AddTotalRow("TOTAL DES RETENUES", b.TotalRetenues, "#c62828");
                table.Cell().ColumnSpan(2).PaddingVertical(2).LineHorizontal(0.5f).LineColor("#cccccc");
                AddTotalRow("SALAIRE BRUT", b.SalaireBrut, "#1a3a5c", true, 10f);
                AddTotalRow("SALAIRE NET IMPOSABLE", b.SalaireNetImposable, "#1a3a5c");
                table.Cell().ColumnSpan(2).PaddingVertical(2).LineHorizontal(1f).LineColor("#1a3a5c");
                AddTotalRow("NET A PAYER", b.SalaireNet, "#1a3a5c", true, 13f);
            });

            // Charges patronales
            if (b.TotalChargesPatronales > 0)
            {
                col.Item().Height(8);
                col.Item().Background("#f5f5f5").Padding(8).Column(c =>
                {
                    c.Item().Text("CHARGES PATRONALES").FontSize(8).Bold().FontColor("#1a3a5c");
                    c.Item().Height(4);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text(t => { t.Span("Total Charges Patronales: ").FontSize(8); t.Span($"{b.TotalChargesPatronales:N2} MAD").FontSize(8).Bold(); });
                        r.RelativeItem().Text(t => { t.Span("Cout Total Employeur: ").FontSize(8); t.Span($"{b.CoutTotalEmployeur:N2} MAD").FontSize(8).Bold().FontColor("#c62828"); });
                    });
                });
            }

            // Cumuls annuels
            if (b.CumulBrutAnnuel > 0)
            {
                col.Item().Height(6);
                col.Item().Background("#fafafa").Border(0.5f).BorderColor("#dddddd").Padding(8).Column(c =>
                {
                    c.Item().Text("CUMULS ANNUELS").FontSize(8).Bold().FontColor("#1a3a5c");
                    c.Item().Height(4);
                    c.Item().Row(r =>
                    {
                        r.RelativeItem().Text(t => { t.Span("Brut: ").FontSize(7.5f).FontColor("#888888"); t.Span($"{b.CumulBrutAnnuel:N2}").FontSize(8); });
                        r.RelativeItem().Text(t => { t.Span("Net Imposable: ").FontSize(7.5f).FontColor("#888888"); t.Span($"{b.CumulNetImposableAnnuel:N2}").FontSize(8); });
                        r.RelativeItem().Text(t => { t.Span("IR: ").FontSize(7.5f).FontColor("#888888"); t.Span($"{b.CumulIRAnnuel:N2}").FontSize(8); });
                        r.RelativeItem().Text(t => { t.Span("CNSS: ").FontSize(7.5f).FontColor("#888888"); t.Span($"{b.CumulCNSSAnnuel:N2}").FontSize(8); });
                    });
                });
            }
        });
    }

    private void ComposeFooter(IContainer container, BulletinCompletDto b)
    {
        container.Column(col =>
        {
            col.Item().Height(6);
            col.Item().Row(row =>
            {
                row.RelativeItem().Text("Document confidentiel - Usage interne uniquement").FontSize(6.5f).Italic().FontColor("#aaaaaa");
                row.RelativeItem().AlignCenter().Text(t =>
                {
                    t.Span("Page ").FontSize(6.5f).FontColor("#aaaaaa");
                    t.CurrentPageNumber().FontSize(6.5f).FontColor("#aaaaaa");
                    t.Span(" / ").FontSize(6.5f).FontColor("#aaaaaa");
                    t.TotalPages().FontSize(6.5f).FontColor("#aaaaaa");
                });
                row.RelativeItem().AlignRight().Text($"Edite le {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(6.5f).Italic().FontColor("#aaaaaa");
            });
        });
    }
}