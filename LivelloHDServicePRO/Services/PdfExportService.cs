using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LivelloHDServicePRO.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LivelloHDServicePRO.Services
{
    /// <summary>
    /// Servizio per esportare report in formato PDF con QuestPDF
    /// </summary>
    public class PdfExportService
    {
        public void EsportaReportPdf(ReportData reportData, string filePath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? Inizio generazione PDF con QuestPDF: {filePath}");
                
                // Configura QuestPDF con font sicuri
                QuestPDF.Settings.License = LicenseType.Community;
                
                // Disabilita il controllo dei glifi per evitare errori con caratteri speciali
                QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
                
                // Pulisci il file se esiste
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        System.Diagnostics.Debug.WriteLine("?? File esistente rimosso");
                    }
                    catch (IOException)
                    {
                        System.Diagnostics.Debug.WriteLine("?? File potrebbe essere in uso, continua comunque");
                    }
                }

                // Sanitizza tutti i dati prima di generare il PDF
                // var sanitizedData = SanitizeReportData(reportData);

                // PROVA: Usa dati originali direttamente per debug
                var dataToUse = reportData;
                
                System.Diagnostics.Debug.WriteLine($"?? Debug - Usando dati con {dataToUse.AnalisiProprietari.Count} proprietari");

                // Genera il PDF
                var document = new ReportDocument(dataToUse);
                document.GeneratePdf(filePath);
                
                System.Diagnostics.Debug.WriteLine("? PDF generato con successo con QuestPDF");
            }
            catch (System.IO.IOException ioEx)
            {
                throw new Exception($"Errore di accesso al file: Assicurati che il file non sia aperto in un'altra applicazione. {ioEx.Message}", ioEx);
            }
            catch (System.UnauthorizedAccessException authEx)
            {
                throw new Exception($"Accesso negato: Verifica i permessi sulla cartella di destinazione. {authEx.Message}", authEx);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Errore durante generazione PDF: {ex}");
                throw new Exception($"Errore durante la generazione del PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Sanitizza tutti i dati del report per evitare problemi con caratteri speciali
        /// </summary>
        private ReportData SanitizeReportData(ReportData original)
        {
            System.Diagnostics.Debug.WriteLine("?? Inizio sanitizzazione dati report...");
            
            // DEBUG: Verifica dati originali
            System.Diagnostics.Debug.WriteLine($"?? Dati originali - Proprietari: {original.AnalisiProprietari.Count}");
            foreach (var prop in original.AnalisiProprietari.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"  - Proprietario: '{prop.NomeProprietario}' ({prop.TotalTickets} ticket)");
            }

            System.Diagnostics.Debug.WriteLine($"?? Dati originali - Proprietari per priorità: {original.AnalisiProprietariPerPriorita.Count}");
            foreach (var prop in original.AnalisiProprietariPerPriorita.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"  - Proprietario: '{prop.NomeProprietario}', Priorità: '{prop.Priorita}' ({prop.NumeroTickets} ticket)");
            }

            var sanitized = new ReportData
            {
                DataGenerazione = original.DataGenerazione,
                TotalTickets = original.TotalTickets,
                TicketsRisolti = original.TicketsRisolti,
                TicketsInCorso = original.TicketsInCorso,
                PeriodoAnalisi = SanitizeText(original.PeriodoAnalisi)
            };

            // Sanitizza tempi medi per priorità
            sanitized.TempiMediPerPriorita = original.TempiMediPerPriorita.Select(t => new TempoMedioPriorita
            {
                Priorita = SanitizeText(t.Priorita),
                NumeroTickets = t.NumeroTickets,
                TempoMedioTMC = t.TempoMedioTMC,
                TempoMedioTMS = t.TempoMedioTMS,
                TempoMedioTEFF = t.TempoMedioTEFF,
                TempoMedioTSOSP = t.TempoMedioTSOSP
            }).ToList();

            // Sanitizza analisi proprietari (MOLTO PIÙ ATTENTO)
            sanitized.AnalisiProprietari = original.AnalisiProprietari.Select(a => new AnalisiProprietario
            {
                NomeProprietario = SanitizeText(a.NomeProprietario),
                TotalTickets = a.TotalTickets,
                TicketsRisolti = a.TicketsRisolti,
                ValutazioneComplessiva = a.ValutazioneComplessiva,
                TempiPerPriorita = a.TempiPerPriorita?.ToDictionary(
                    kvp => SanitizeText(kvp.Key),
                    kvp => kvp.Value
                ) ?? new Dictionary<string, TempiProprietario>()
            }).ToList();

            System.Diagnostics.Debug.WriteLine($"? Dopo sanitizzazione - Proprietari: {sanitized.AnalisiProprietari.Count}");
            foreach (var prop in sanitized.AnalisiProprietari.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"  - Sanitizzato: '{prop.NomeProprietario}' ({prop.TotalTickets} ticket)");
            }

            // Sanitizza analisi proprietari per priorità
            sanitized.AnalisiProprietariPerPriorita = original.AnalisiProprietariPerPriorita.Select(a => new AnalisiProprietarioPriorita
            {
                NomeProprietario = SanitizeText(a.NomeProprietario),
                Priorita = SanitizeText(a.Priorita),
                NumeroTickets = a.NumeroTickets,
                TicketsRisolti = a.TicketsRisolti,
                TempoMedioTMC = a.TempoMedioTMC,
                TempoMedioTMS = a.TempoMedioTMS,
                TempoMedioTEFF = a.TempoMedioTEFF,
                TempoMedioTSOSP = a.TempoMedioTSOSP,
                TicketsFuoriSLA = a.TicketsFuoriSLA,
                ValutazionePriorita = a.ValutazionePriorita
            }).ToList();

            System.Diagnostics.Debug.WriteLine($"? Dopo sanitizzazione - Proprietari per priorità: {sanitized.AnalisiProprietariPerPriorita.Count}");
            foreach (var prop in sanitized.AnalisiProprietariPerPriorita.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"  - Sanitizzato: '{prop.NomeProprietario}', Priorità: '{prop.Priorita}' ({prop.NumeroTickets} ticket)");
            }

            // Continua con il resto dei dati...
            sanitized.DistribuzionePriorita = original.DistribuzionePriorita.Select(d => new DistribuzionePriorita
            {
                Priorita = SanitizeText(d.Priorita),
                NumeroTickets = d.NumeroTickets,
                Percentuale = d.Percentuale,
                TicketsFuoriSLA = d.TicketsFuoriSLA
            }).ToList();

            // Sanitizza distribuzione proprietario
            sanitized.DistribuzioneProprietario = original.DistribuzioneProprietario.Select(d => new DistribuzioneProprietario
            {
                Proprietario = SanitizeText(d.Proprietario),
                NumeroTickets = d.NumeroTickets,
                Percentuale = d.Percentuale,
                TicketsRisolti = d.TicketsRisolti
            }).ToList();

            // Sanitizza performance SLA
            sanitized.SlaPerformanceData = original.SlaPerformanceData.Select(s => new SlaPerformance
            {
                Categoria = SanitizeText(s.Categoria),
                TicketsTotali = s.TicketsTotali,
                TicketsEntroSLA = s.TicketsEntroSLA,
                TicketsFuoriSLA = s.TicketsFuoriSLA
            }).ToList();

            // Sanitizza riepilogo generale
            sanitized.RiepilogoGenerale = new RiepilogoStatistiche
            {
                TempoMedioGlobaleTMC = original.RiepilogoGenerale.TempoMedioGlobaleTMC,
                TempoMedioGlobaleTMS = original.RiepilogoGenerale.TempoMedioGlobaleTMS,
                TempoMedioGlobaleTEFF = original.RiepilogoGenerale.TempoMedioGlobaleTEFF,
                PercentualeComplessivaEntroSLA = original.RiepilogoGenerale.PercentualeComplessivaEntroSLA,
                PercentualeComplessivaFuoriSLA = original.RiepilogoGenerale.PercentualeComplessivaFuoriSLA,
                ProprietarioMigliore = SanitizeText(original.RiepilogoGenerale.ProprietarioMigliore),
                ProprietarioCritico = SanitizeText(original.RiepilogoGenerale.ProprietarioCritico),
                PrioritaPiuProblematica = SanitizeText(original.RiepilogoGenerale.PrioritaPiuProblematica),
                PrioritaMenoProblematica = SanitizeText(original.RiepilogoGenerale.PrioritaMenoProblematica)
            };

            System.Diagnostics.Debug.WriteLine("? Sanitizzazione completata");
            return sanitized;
        }

        /// <summary>
        /// Sanitizza il testo per evitare caratteri problematici nel PDF
        /// </summary>
        public string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "N/A";
            
            System.Diagnostics.Debug.WriteLine($"?? Sanitizing: '{text}'");
            
            // Prima fase: sostituisci caratteri accentati
            var sanitized = text
                .Replace("à", "a").Replace("è", "e").Replace("é", "e")
                .Replace("ì", "i").Replace("ò", "o").Replace("ù", "u")
                .Replace("À", "A").Replace("È", "E").Replace("É", "E")
                .Replace("Ì", "I").Replace("Ò", "O").Replace("Ù", "U")
                .Replace("ç", "c").Replace("Ç", "C")
                .Trim();

            // Seconda fase: rimuovi solo emoji specifiche (MENO AGGRESSIVO)
            var result = sanitized
                .Replace("??", "")
                .Replace("??", "")
                .Replace("??", "")
                .Replace("??", "")
                .Replace("??", "")
                .Replace("??", "")
                .Replace("?", "")
                .Replace("??", "")
                .Replace("??", "")
                .Replace("??", "")
                .Replace("??", "")
                .Replace("??", "")
                .Replace("??", "")
                .Replace("??", "")
                .Trim();

            // Se il testo è ancora valido dopo la pulizia, restituiscilo
            if (!string.IsNullOrWhiteSpace(result))
            {
                System.Diagnostics.Debug.WriteLine($"? Sanitized result: '{result}'");
                return result;
            }

            // Solo se tutto è stato rimosso, restituisci l'originale o N/A
            System.Diagnostics.Debug.WriteLine($"?? Text became empty after sanitization, returning original: '{text}'");
            return string.IsNullOrWhiteSpace(text) ? "N/A" : text;
        }

        /// <summary>
        /// Esporta l'analisi dettagliata di una risorsa in PDF
        /// </summary>
        public void EsportaAnalisiRisorsaPdf(AnalisiDettagliataRisorsa risorsa, string filePath)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Inizio generazione PDF analisi risorsa: {filePath}");
                
                QuestPDF.Settings.License = LicenseType.Community;
                QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                var document = new AnalisiRisorsaDocument(risorsa);
                document.GeneratePdf(filePath);
                
                System.Diagnostics.Debug.WriteLine("PDF analisi risorsa generato con successo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Errore durante generazione PDF: {ex}");
                throw new Exception($"Errore durante la generazione del PDF: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Classe per generare il documento PDF con QuestPDF
    /// </summary>
    public class ReportDocument : IDocument
    {
        private readonly ReportData _reportData;

        public ReportDocument(ReportData reportData)
        {
            _reportData = reportData ?? throw new ArgumentNullException(nameof(reportData));
        }

        public DocumentMetadata GetMetadata() => new DocumentMetadata()
        {
            Title = "Report di Analisi - Qualita del Servizio",
            Author = "Livello HD Service PRO",
            Subject = "Analisi SLA",
            Keywords = "SLA, Report, Analisi, Qualita, Servizio",
            Creator = "Livello HD Service PRO"
        };

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // Header
                    page.Header().Element(ComposeHeader);

                    // Content
                    page.Content().Element(ComposeContent);

                    // Footer
                    page.Footer().Element(ComposeFooter);
                });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                // Titolo principale
                column.Item().AlignCenter().Text("REPORT DI ANALISI - QUALITA DEL SERVIZIO")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                column.Item().AlignCenter().Text($"Periodo: {_reportData.PeriodoAnalisi}")
                    .FontSize(12).FontColor(Colors.Grey.Darken1);

                column.Item().AlignCenter().Text($"Generato il: {_reportData.DataGenerazione:dd/MM/yyyy HH:mm}")
                    .FontSize(10).FontColor(Colors.Grey.Darken1);

                column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Medium);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(15);

                // Statistiche generali
                column.Item().Element(c => ComposeStatisticheGenerali(c));

                // Tempi medi per priorità
                column.Item().Element(c => ComposeTempiMediPerPriorita(c));

                // Performance SLA
                column.Item().Element(c => ComposePerformanceSla(c));

                // Analisi proprietari (top 15)
                column.Item().Element(c => ComposeAnalisiProprietari(c));

                // Analisi proprietari per priorità (top 20)
                column.Item().Element(c => ComposeAnalisiProprietariPerPriorita(c));

                // Distribuzione
                column.Item().Element(c => ComposeDistribuzione(c));

                // Riepilogo esecutivo
                column.Item().Element(c => ComposeRiepilogoEsecutivo(c));
            });
        }

        private void ComposeStatisticheGenerali(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("STATISTICHE GENERALI").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text(_reportData.TotalTickets.ToString()).FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("TICKET TOTALI").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text(_reportData.TicketsRisolti.ToString()).FontSize(24).SemiBold().FontColor(Colors.Green.Medium);
                        col.Item().Text("RISOLTI").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text($"{_reportData.RiepilogoGenerale.PercentualeComplessivaEntroSLA:F1}%").FontSize(24).SemiBold().FontColor(Colors.Orange.Medium);
                        col.Item().Text("ENTRO SLA").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().AlignCenter().Column(col =>
                    {
                        col.Item().Text($"{_reportData.RiepilogoGenerale.PercentualeComplessivaFuoriSLA:F1}%").FontSize(24).SemiBold().FontColor(Colors.Red.Medium);
                        col.Item().Text("FUORI SLA").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }

        private void ComposeTempiMediPerPriorita(IContainer container)
        {
            if (!_reportData.TempiMediPerPriorita.Any()) return;

            container.Column(column =>
            {
                column.Item().Text("TEMPI MEDI PER PRIORITA").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                
                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Priorita");
                        header.Cell().Element(HeaderCell).Text("# Ticket");
                        header.Cell().Element(HeaderCell).Text("TMC Medio");
                        header.Cell().Element(HeaderCell).Text("T-EFF Medio");
                    });

                    // Dati
                    foreach (var item in _reportData.TempiMediPerPriorita.Take(10))
                    {
                        table.Cell().Element(DataCell).Text(SanitizeText(item.Priorita));
                        table.Cell().Element(DataCell).Text(item.NumeroTickets.ToString());
                        table.Cell().Element(DataCell).Text(item.TempoMedioTMCFormatted);
                        table.Cell().Element(DataCell).Text(item.TempoMedioTEFFFormatted);
                    }
                });
            });
        }

        private void ComposePerformanceSla(IContainer container)
        {
            if (!_reportData.SlaPerformanceData.Any()) return;

            container.Column(column =>
            {
                column.Item().Text("PERFORMANCE SLA").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                
                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Categoria");
                        header.Cell().Element(HeaderCell).Text("Totali");
                        header.Cell().Element(HeaderCell).Text("% Entro");
                        header.Cell().Element(HeaderCell).Text("% Fuori");
                    });

                    // Dati
                    foreach (var item in _reportData.SlaPerformanceData)
                    {
                        table.Cell().Element(DataCell).Text(SanitizeText(item.Categoria));
                        table.Cell().Element(DataCell).Text(item.TicketsTotali.ToString());
                        table.Cell().Element(DataCell).Text($"{item.PercentualeEntroSLA:F1}%");
                        table.Cell().Element(DataCell).Text($"{item.PercentualeFuoriSLA:F1}%");
                    }
                });
            });
        }

        private void ComposeAnalisiProprietari(IContainer container)
        {
            if (!_reportData.AnalisiProprietari.Any()) return;

            container.Column(column =>
            {
                column.Item().Text("ANALISI PROPRIETARI (TOP 15)").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                
                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Proprietario");
                        header.Cell().Element(HeaderCell).Text("# Ticket");
                        header.Cell().Element(HeaderCell).Text("% Risoluzione");
                        header.Cell().Element(HeaderCell).Text("Valutazione");
                    });

                    // Dati
                    foreach (var item in _reportData.AnalisiProprietari.Take(15))
                    {
                        table.Cell().Element(DataCell).Text(SanitizeText(item.NomeProprietario));
                        table.Cell().Element(DataCell).Text(item.TotalTickets.ToString());
                        table.Cell().Element(DataCell).Text($"{item.PercentualeRisoluzione:F1}%");
                        table.Cell().Element(DataCell).Text(SanitizeText(item.DescrizioneValutazione));
                    }
                });
            });
        }

        private void ComposeAnalisiProprietariPerPriorita(IContainer container)
        {
            if (!_reportData.AnalisiProprietariPerPriorita.Any()) return;

            container.Column(column =>
            {
                column.Item().Text("ANALISI PROPRIETARI PER PRIORITA (TOP 20)").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                
                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn(0.8f);
                        columns.RelativeColumn(0.8f);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCell).Text("Proprietario");
                        header.Cell().Element(HeaderCell).Text("Priorita");
                        header.Cell().Element(HeaderCell).Text("# TK");
                        header.Cell().Element(HeaderCell).Text("% Ris.");
                        header.Cell().Element(HeaderCell).Text("TMC");
                        header.Cell().Element(HeaderCell).Text("T-EFF");
                        header.Cell().Element(HeaderCell).Text("Valutazione");
                    });

                    // Dati
                    foreach (var item in _reportData.AnalisiProprietariPerPriorita.Take(20))
                    {
                        table.Cell().Element(DataCell).Text(SanitizeText(item.NomeProprietario)).FontSize(8);
                        table.Cell().Element(DataCell).Text(SanitizeText(item.Priorita)).FontSize(8);
                        table.Cell().Element(DataCell).Text(item.NumeroTickets.ToString()).FontSize(8);
                        table.Cell().Element(DataCell).Text($"{item.PercentualeRisoluzione:F1}%").FontSize(8);
                        table.Cell().Element(DataCell).Text(item.TempoMedioTMCFormatted).FontSize(8);
                        table.Cell().Element(DataCell).Text(item.TempoMedioTEFFFormatted).FontSize(8);
                        table.Cell().Element(DataCell).Text(SanitizeText(item.DescrizioneValutazionePriorita)).FontSize(8);
                    }
                });
            });
        }

        private void ComposeDistribuzione(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("DISTRIBUZIONE").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                
                column.Item().PaddingTop(5).Row(row =>
                {
                    // Distribuzione Priorità
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Distribuzione Priorita").FontSize(12).SemiBold();
                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellSmall).Text("Priorita");
                                header.Cell().Element(HeaderCellSmall).Text("# Ticket");
                                header.Cell().Element(HeaderCellSmall).Text("% Fuori SLA");
                            });

                            foreach (var item in _reportData.DistribuzionePriorita.Take(8))
                            {
                                table.Cell().Element(DataCellSmall).Text(SanitizeText(item.Priorita));
                                table.Cell().Element(DataCellSmall).Text(item.NumeroTickets.ToString());
                                table.Cell().Element(DataCellSmall).Text($"{item.PercentualeFuoriSLA:F1}%");
                            }
                        });
                    });

                    // Padding tra le colonne
                    row.ConstantItem(20);

                    // Top Proprietari
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Top Proprietari").FontSize(12).SemiBold();
                        col.Item().PaddingTop(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellSmall).Text("Proprietario");
                                header.Cell().Element(HeaderCellSmall).Text("# Ticket");
                                header.Cell().Element(HeaderCellSmall).Text("% Risolti");
                            });

                            foreach (var item in _reportData.DistribuzioneProprietario.Take(8))
                            {
                                table.Cell().Element(DataCellSmall).Text(SanitizeText(item.Proprietario));
                                table.Cell().Element(DataCellSmall).Text(item.NumeroTickets.ToString());
                                table.Cell().Element(DataCellSmall).Text($"{item.PercentualeRisoluzione:F1}%");
                            }
                        });
                    });
                });
            });
        }

        private void ComposeRiepilogoEsecutivo(IContainer container)
        {
            var riepilogo = _reportData.RiepilogoGenerale;

            container.Column(column =>
            {
                column.Item().Text("RIEPILOGO ESECUTIVO").FontSize(14).SemiBold().FontColor(Colors.Blue.Medium);
                
                column.Item().PaddingTop(5).Row(row =>
                {
                    // Tempi medi globali
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("TEMPI MEDI GLOBALI").FontSize(12).SemiBold();
                        col.Item().Text($"TMC: {riepilogo.TempoMedioGlobaleTMCFormatted}").FontColor(Colors.Blue.Medium);
                        col.Item().Text($"T-EFF: {riepilogo.TempoMedioGlobaleTEFFFormatted}").FontColor(Colors.Orange.Medium);
                    });

                    row.ConstantItem(20);

                    // Performance
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("MIGLIORI PERFORMANCE").FontSize(12).SemiBold();
                        col.Item().Text($"Migliore: {SanitizeText(riepilogo.ProprietarioMigliore)}").FontColor(Colors.Green.Medium);
                        col.Item().Text($"Da migliorare: {SanitizeText(riepilogo.ProprietarioCritico)}").FontColor(Colors.Red.Medium);
                    });
                });

                column.Item().PaddingTop(10).Column(col =>
                {
                    col.Item().Text("PRIORITA").FontSize(12).SemiBold();
                    col.Item().Text($"Piu problematica: {SanitizeText(riepilogo.PrioritaPiuProblematica)} | Meno problematica: {SanitizeText(riepilogo.PrioritaMenoProblematica)}");
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text("Livello HD Service PRO - Report di Analisi")
                .FontSize(8).FontColor(Colors.Grey.Darken1);
        }

        // Metodi helper per stili
        private IContainer HeaderCell(IContainer container) => container
            .Border(1).BorderColor(Colors.Blue.Medium)
            .Background(Colors.Blue.Lighten4)
            .Padding(5).AlignCenter();

        private IContainer HeaderCellSmall(IContainer container) => container
            .Border(1).BorderColor(Colors.Blue.Medium)
            .Background(Colors.Blue.Lighten4)
            .Padding(3).AlignCenter();

        private IContainer DataCell(IContainer container) => container
            .Border(1).BorderColor(Colors.Grey.Lighten2)
            .Padding(5).AlignCenter();

        private IContainer DataCellSmall(IContainer container) => container
            .Border(1).BorderColor(Colors.Grey.Lighten2)
            .Padding(3).AlignCenter();

        /// <summary>
        /// Usa il metodo SanitizeText dalla classe PdfExportService
        /// </summary>
        private string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text)) return "N/A";
            
            // Versione semplificata per evitare duplicazioni
            return text
                .Replace("à", "a").Replace("è", "e").Replace("é", "e")
                .Replace("ì", "i").Replace("ò", "o").Replace("ù", "u")
                .Replace("À", "A").Replace("È", "E").Replace("É", "E")
                .Replace("Ì", "I").Replace("Ò", "O").Replace("Ù", "U")
                .Replace("??", "").Replace("??", "").Replace("??", "")
                .Replace("??", "").Replace("??", "")
                .Trim();
        }
    }

    /// <summary>
    /// Documento PDF per l'analisi dettagliata di una risorsa
    /// </summary>
    public class AnalisiRisorsaDocument : IDocument
    {
        private readonly AnalisiDettagliataRisorsa _risorsa;

        public AnalisiRisorsaDocument(AnalisiDettagliataRisorsa risorsa)
        {
            _risorsa = risorsa;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Pagina ");
                    x.CurrentPageNumber();
                    x.Span(" di ");
                    x.TotalPages();
                });
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Background("#9C27B0").Padding(10).Text(text =>
                {
                    text.Span("ANALISI DETTAGLIATA RISORSA\n").FontSize(18).Bold().FontColor(Colors.White);
                    text.Span(_risorsa.NomeProprietario).FontSize(14).FontColor(Colors.White);
                });

                column.Item().PaddingVertical(5);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(column =>
            {
                // Statistiche principali
                column.Item().Element(ComposeStatistichePrincipali);
                column.Item().PaddingVertical(5);

                // Sintesi analitica
                column.Item().Element(ComposeSintesi);
                column.Item().PaddingVertical(5);

                // Tempi medi
                column.Item().Element(ComposeTempiMedi);
                column.Item().PaddingVertical(5);

                // Performance SLA
                column.Item().Element(ComposePerformanceSla);
                column.Item().PaddingVertical(5);

                // Punti di forza
                if (_risorsa.PuntiDiForza.Any())
                {
                    column.Item().Element(ComposePuntiForza);
                    column.Item().PaddingVertical(5);
                }

                // Aree di miglioramento
                if (_risorsa.AreeDiMiglioramento.Any())
                {
                    column.Item().Element(ComposeAreeMiglioramento);
                    column.Item().PaddingVertical(5);
                }

                // Suggerimenti
                if (_risorsa.SuggerimentiAzioni.Any())
                {
                    column.Item().Element(ComposeSuggerimenti);
                    column.Item().PaddingVertical(5);
                }

                // Distribuzione priorita
                column.Item().Element(ComposeDistribuzionePriorita);
                column.Item().PaddingVertical(5);

                // Tendenze mensili
                if (_risorsa.TendenzeMensili.Any())
                {
                    column.Item().Element(ComposeTendenzeMensili);
                }
            });
        }

        private void ComposeStatistichePrincipali(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("STATISTICHE PRINCIPALI").FontSize(12).Bold().FontColor("#9C27B0");
                column.Item().PaddingVertical(3);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Valutazione:").Bold();
                        col.Item().Text(_risorsa.DescrizioneValutazione);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Ticket Totali:").Bold();
                        col.Item().Text(_risorsa.TicketTotali.ToString());
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("% Risoluzione:").Bold();
                        col.Item().Text($"{_risorsa.PercentualeRisoluzione:F1}%");
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Posizione:").Bold();
                        col.Item().Text(_risorsa.PosizioneRelativa);
                    });
                });
            });
        }

        private void ComposeSintesi(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("SINTESI ANALITICA").FontSize(12).Bold().FontColor("#9C27B0");
                column.Item().PaddingVertical(3);
                column.Item().Text(_risorsa.SintesiAnalitica ?? "Nessuna sintesi disponibile.");
            });
        }

        private void ComposeTempiMedi(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("TEMPI MEDI").FontSize(12).Bold().FontColor("#9C27B0");
                column.Item().PaddingVertical(3);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("TMC:").Bold();
                        col.Item().Text(_risorsa.TempoMedioTMCFormatted);
                        col.Item().Text($"Deviazione: {FormatDeviazione(_risorsa.DeviazioneDallaMediaTMC)}").FontSize(9);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("T-EFF:").Bold();
                        col.Item().Text(_risorsa.TempoMedioTEFFFormatted);
                        col.Item().Text($"Deviazione: {FormatDeviazione(_risorsa.DeviazioneDallaMediaTEFF)}").FontSize(9);
                    });
                });
            });
        }

        private void ComposePerformanceSla(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("PERFORMANCE SLA").FontSize(12).Bold().FontColor("#9C27B0");
                column.Item().PaddingVertical(3);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Entro SLA:").Bold();
                        col.Item().Text($"{_risorsa.TicketsEntroSLA} ({_risorsa.PercentualeEntroSLA:F1}%)");
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Fuori SLA:").Bold();
                        col.Item().Text($"{_risorsa.TicketsFuoriSLA} ({_risorsa.PercentualeFuoriSLA:F1}%)");
                    });
                });
            });
        }

        private void ComposePuntiForza(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("PUNTI DI FORZA").FontSize(12).Bold().FontColor("#4CAF50");
                column.Item().PaddingVertical(3);

                foreach (var punto in _risorsa.PuntiDiForza)
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(15).Text("•");
                        row.RelativeItem().Text(punto);
                    });
                }
            });
        }

        private void ComposeAreeMiglioramento(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("AREE DI MIGLIORAMENTO").FontSize(12).Bold().FontColor("#FF9800");
                column.Item().PaddingVertical(3);

                foreach (var area in _risorsa.AreeDiMiglioramento)
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(15).Text("•");
                        row.RelativeItem().Text(area);
                    });
                }
            });
        }

        private void ComposeSuggerimenti(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("SUGGERIMENTI PER AZIONI").FontSize(12).Bold().FontColor("#2196F3");
                column.Item().PaddingVertical(3);

                foreach (var suggerimento in _risorsa.SuggerimentiAzioni)
                {
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(15).Text("•");
                        row.RelativeItem().Text(suggerimento);
                    });
                }
            });
        }

        private void ComposeDistribuzionePriorita(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("DISTRIBUZIONE PER PRIORITA").FontSize(12).Bold().FontColor("#9C27B0");
                column.Item().PaddingVertical(3);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#E0E0E0").Padding(5).Text("Priorita").Bold();
                        header.Cell().Background("#E0E0E0").Padding(5).Text("Numero Ticket").Bold();
                    });

                    foreach (var item in _risorsa.TicketsPerPriorita)
                    {
                        table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Text(item.Key);
                        table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Text(item.Value.ToString());
                    }
                });
            });
        }

        private void ComposeTendenzeMensili(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("TENDENZE MENSILI").FontSize(12).Bold().FontColor("#9C27B0");
                column.Item().PaddingVertical(3);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#E0E0E0").Padding(5).Text("Mese").Bold();
                        header.Cell().Background("#E0E0E0").Padding(5).Text("Ticket").Bold();
                        header.Cell().Background("#E0E0E0").Padding(5).Text("% Risoluzione").Bold();
                        header.Cell().Background("#E0E0E0").Padding(5).Text("Tendenza").Bold();
                    });

                    foreach (var tendenza in _risorsa.TendenzeMensili)
                    {
                        table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Text(tendenza.MeseDisplay);
                        table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Text(tendenza.TicketsGestiti.ToString());
                        table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Text($"{tendenza.PercentualeRisoluzione:F1}%");
                        table.Cell().BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Text(tendenza.TendenzaDescrizione ?? "");
                    }
                });
            });
        }

        private string FormatDeviazione(double deviazione)
        {
            var segno = deviazione >= 0 ? "+" : "";
            return $"{segno}{deviazione:F1}% dalla media";
        }
    }
}