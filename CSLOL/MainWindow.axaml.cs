using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace CSLOL
{
    public partial class MainWindow : Window
    {
        private string _playerPseudo = string.Empty;
        private string _selectedChampion = string.Empty;
        private string _selectedRace = string.Empty;
        private string _selectedClass = string.Empty;
        private List<ItemData> _selectedItems = new();
// Lien vers l'API de LoL pour récupérer les items        
        private static readonly HttpClient _httpClient = new();
        private const string ITEM_JSON_URL = "https://ddragon.leagueoflegends.com/cdn/14.2.1/data/fr_FR/item.json";
        private const string ITEM_IMAGE_BASE_URL = "https://ddragon.leagueoflegends.com/cdn/14.2.1/img/item/";
// Dico des stats
        private readonly Dictionary<string, ChampionStats> _championStats = new()
        {
            { "Garen", new ChampionStats("Garen", "Humain", 620, 66, 0, 0.63, 36, 32, 340, 8.0, 
                "Justice de Demacia", "Active (Ultime)", 
                "Invoque une épée géante depuis les cieux pour s'abattre sur un ennemi. Inflige des dégâts bruts équivalents à 30% des PV manquants de la cible. \"Pour la cause !\"") },
            
            { "Teemo", new ChampionStats("Teemo", "Yordle", 590, 54, 15, 0.69, 24, 30, 330, 5.5,
                "Guérilla", "Passive",
                "Si le personnage reste immobile pendant 2 tours, il devient invisible. Sa première attaque en sortant de l'invisibilité bénéficie d'un bonus de 40% en Vitesse d'Attaque.") },
            
            { "Ahri", new ChampionStats("Ahri", "Vastaya", 530, 53, 20, 0.66, 21, 30, 330, 5.5,
                "Charme Spirituel", "Active (Contrôle)",
                "Lance un baiser qui inflige des dégâts magiques et force l'ennemi à marcher inoffensivement vers Ahri pendant 2 secondes, réduisant sa défense de 20%.") },
            
            { "KhaZix", new ChampionStats("Kha'Zix", "Néant", 570, 63, 0, 0.67, 36, 32, 350, 7.5,
                "Menace Invisible", "Passive",
                "Lorsqu'il n'est pas vu par l'équipe ennemie, la prochaine attaque de base inflige des dégâts physiques supplémentaires et ralentit la cible.") },
            
            { "Thresh", new ChampionStats("Thresh", "Mort-vivant", 600, 50, 0, 0.63, 28, 30, 335, 7.0,
                "Damnation", "Passive (Collecte)",
                "Les ennemis morts près de Thresh laissent parfois tomber une âme. Collecter une âme augmente définitivement l'Armure et la Puissance de 0.75 point.") },
            
            { "AurelionSol", new ChampionStats("Aurelion Sol", "Céleste", 620, 55, 25, 0.63, 22, 30, 325, 7.0,
                "Créateur d'Étoiles", "Active (Zone)",
                "Crée une galaxie miniature qui grandit tant qu'elle voyage avec lui. À l'impact, elle explose en étourdissant les ennemis sur une large zone. La taille de la zone dépend de la distance parcourue.") },
            
            { "Lux", new ChampionStats("Lux", "Humain", 560, 54, 25, 0.63, 19, 30, 330, 5.5,
                "Éclat Final", "Active (Magique)",
                "Tire un laser de lumière concentrée qui traverse toutes les cibles sur une ligne. Inflige des dégâts magiques massifs et révèle les zones d'ombre pendant 5 secondes.") },
            
            { "Veigar", new ChampionStats("Veigar", "Yordle", 550, 52, 30, 0.63, 21, 30, 340, 6.5,
                "Phénoménal Maléfique", "Passive (Évolutive)",
                "Gagne +1 AP (Puissance) de manière permanente pour chaque ennemi vaincu. Il n'y a aucune limite à la puissance que vous pouvez accumuler.") },
            
            { "VelKoz", new ChampionStats("Vel'Koz", "Néant", 590, 55, 30, 0.63, 22, 30, 340, 5.5,
                "Désintégration Organique", "Active (Brut)",
                "Tire un rayon d'énergie pure qui peut être divisé à 90 degrés. Chaque troisième sort touchant la même cible inflige des dégâts bruts ignorant l'armure et la résistance magique.") },
            
            { "Mordekaiser", new ChampionStats("Mordekaiser", "Mort-vivant", 645, 61, 20, 0.63, 37, 32, 335, 8.5,
                "Royaume des Morts", "Active (Duel)",
                "Bannit la cible dans une autre dimension pour un duel en 1 contre 1 pendant 7 secondes, volant 10% de ses stats actuelles pendant la durée du combat.") }
        };
// Dico des races
        private readonly Dictionary<string, string> _raceBlockedClass = new()
        {
            { "Humain", "Assassin" },
            { "Yordle", "Tank" },
            { "Vastaya", "Tank" },
            { "Néant", "Support" },
            { "Mort-vivant", "Support" },
            { "Céleste", "Combattant" }
        };
// Dico des classes
        private readonly Dictionary<string, ClassBonus> _classBonus = new()
        {
            { "Combattant", new ClassBonus(50, 10, 0, 0, 0, 0, 0, 0) },
            { "Mage", new ClassBonus(0, 0, 15, 0, 0, 0, 0, 0.05) },
            { "Assassin", new ClassBonus(0, 10, 0, 0, 0, 0, 15, 0) },
            { "Tank", new ClassBonus(100, 0, 0, 0, 15, 0, 0, 0) },
            { "Support", new ClassBonus(0, 0, 0, 0, 0, 10, 0, 5.0) }
        };

        private Button? _lastSelectedChampionButton;
        private Button? _lastSelectedClassButton;
//Dico des bottes (pour les items)
        private readonly HashSet<string> _bootsIds = new()
        {
            "3006", "3047", "3020", "3111", "3117", "3009"
        };

        public MainWindow()
        {
            InitializeComponent();
            
            PseudoInput.TextChanged += (sender, e) =>
            {
                ContinueButton.IsEnabled = !string.IsNullOrWhiteSpace(PseudoInput.Text);
            };
            
            ContinueButton.IsEnabled = false;
            
            PseudoInput.KeyDown += (sender, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter && ContinueButton.IsEnabled)
                {
                    OnContinueClick(sender, null);
                }
            };
        }
// Entrée du pseudo
        private void OnContinueClick(object? sender, RoutedEventArgs? e)
        {
            _playerPseudo = PseudoInput.Text?.Trim() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(_playerPseudo))
                return;

            Page1.IsVisible = false;
            Page2.IsVisible = true;
        }

        private void OnChampionClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string championTag)
                return;

            if (_lastSelectedChampionButton != null)
            {
                _lastSelectedChampionButton.Background = Avalonia.Media.Brushes.Transparent;
            }

            button.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0f3460"));
            _lastSelectedChampionButton = button;

            _selectedChampion = championTag;

            if (!_championStats.TryGetValue(championTag, out var stats))
                return;

            StatsPanel.IsVisible = true;
            DefaultMessage.IsVisible = false;

            try
            {
                TopChampionBanner.Source = new Bitmap($"Assets/{championTag}_0.jpg");
                TopChampionName.Text = stats.Name;
            }
            catch
            {
                try
                {
                    TopChampionBanner.Source = new Bitmap($"Assets/{championTag}.jpg");
                    TopChampionName.Text = stats.Name;
                }
                catch
                {
                    TopChampionName.Text = "Image introuvable";
                }
            }
    
            TopChampionRace.Text = stats.Race;

            StatPV.Text = stats.PV.ToString();
            StatAD.Text = stats.AD.ToString();
            StatAP.Text = stats.AP.ToString();
            StatAS.Text = stats.AS.ToString("0.00");
            StatArmor.Text = stats.Armor.ToString();
            StatRM.Text = stats.RM.ToString();
            StatMS.Text = stats.MS.ToString();
            StatRegen.Text = stats.Regen.ToString("0.0");

            AbilityName.Text = stats.AbilityName;
            AbilityType.Text = stats.AbilityType;
            AbilityDescription.Text = stats.AbilityDescription;
        }

        private void OnSelectChampionClick(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedChampion))
                return;

            if (_championStats.TryGetValue(_selectedChampion, out var stats))
            {
                _selectedRace = stats.Race;
                
                SelectedChampionDisplay.Text = stats.Name;
                SelectedRaceDisplay.Text = stats.Race;
                
                ClassChampionName.Text = stats.Name;
                ClassChampionRace.Text = stats.Race;
                
                try
                {
                    ClassChampionBanner.Source = new Bitmap($"Assets/{_selectedChampion}_0.jpg");
                }
                catch
                {
                    try
                    {
                        ClassChampionBanner.Source = new Bitmap($"Assets/{_selectedChampion}.jpg");
                    }
                    catch { }
                }
                
                ClassFighter.IsEnabled = true;
                ClassMage.IsEnabled = true;
                ClassAssassin.IsEnabled = true;
                ClassTank.IsEnabled = true;
                ClassSupport.IsEnabled = true;
                
                if (_lastSelectedClassButton != null)
                {
                    _lastSelectedClassButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#16213e"));
                }
                _lastSelectedClassButton = null;
                _selectedClass = string.Empty;
                
                ClassStatsPanel.IsVisible = false;
                ClassDefaultMessage.IsVisible = true;
                
                if (_raceBlockedClass.TryGetValue(_selectedRace, out var blockedClass))
                {
                    switch (blockedClass)
                    {
                        case "Assassin":
                            ClassAssassin.IsEnabled = false;
                            break;
                        case "Tank":
                            ClassTank.IsEnabled = false;
                            break;
                        case "Support":
                            ClassSupport.IsEnabled = false;
                            break;
                        case "Combattant":
                            ClassFighter.IsEnabled = false;
                            break;
                    }
                }
            }

            Page2.IsVisible = false;
            Page3.IsVisible = true;
        }

        private void OnClassClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string classTag)
                return;

            if (_lastSelectedClassButton != null)
            {
                _lastSelectedClassButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#16213e"));
            }

            button.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0f3460"));
            _lastSelectedClassButton = button;

            _selectedClass = classTag;

            if (_championStats.TryGetValue(_selectedChampion, out var stats) && 
                _classBonus.TryGetValue(classTag, out var bonus))
            {
                ClassStatsPanel.IsVisible = true;
                ClassDefaultMessage.IsVisible = false;

                ClassStatPV.Text = stats.PV.ToString();
                ClassStatAD.Text = stats.AD.ToString();
                ClassStatAP.Text = stats.AP.ToString();
                ClassStatAS.Text = stats.AS.ToString("0.00");
                ClassStatArmor.Text = stats.Armor.ToString();
                ClassStatRM.Text = stats.RM.ToString();
                ClassStatMS.Text = stats.MS.ToString();
                ClassStatRegen.Text = stats.Regen.ToString("0.0");

                ClassBonusPV.Text = bonus.BonusPV > 0 ? $"+{bonus.BonusPV}" : "";
                ClassBonusAD.Text = bonus.BonusAD > 0 ? $"+{bonus.BonusAD}" : "";
                ClassBonusAP.Text = bonus.BonusAP > 0 ? $"+{bonus.BonusAP}" : "";
                ClassBonusAS.Text = "";
                ClassBonusArmor.Text = bonus.BonusArmor > 0 ? $"+{bonus.BonusArmor}" : "";
                ClassBonusRM.Text = bonus.BonusRM > 0 ? $"+{bonus.BonusRM}" : "";
                ClassBonusMS.Text = bonus.BonusMS > 0 ? $"+{bonus.BonusMS}" : "";
                
                if (bonus.BonusRegen > 0)
                {
                    if (classTag == "Mage")
                        ClassBonusRegen.Text = $"+{bonus.BonusRegen * 100:0}%";
                    else
                        ClassBonusRegen.Text = $"+{bonus.BonusRegen:0.0}";
                }
                else
                {
                    ClassBonusRegen.Text = "";
                }
            }
        }

        private async void OnContinueToItemsClick(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedClass))
                return;

            Page3.IsVisible = false;
            Page4.IsVisible = true;
            
            await LoadRandomItems();
        }

        private void OnBackToChampionClick(object? sender, RoutedEventArgs e)
        {
            Page3.IsVisible = false;
            Page2.IsVisible = true;
        }

        private FinalStats CalculateFinalStats()
        {
            if (!_championStats.TryGetValue(_selectedChampion, out var championStats))
                return new FinalStats();

            if (!_classBonus.TryGetValue(_selectedClass, out var classBonus))
                return new FinalStats();

            var finalStats = new FinalStats
            {
                PV = championStats.PV + classBonus.BonusPV,
                AD = championStats.AD + classBonus.BonusAD,
                AP = championStats.AP + classBonus.BonusAP,
                AS = championStats.AS,
                Armor = championStats.Armor + classBonus.BonusArmor,
                RM = championStats.RM + classBonus.BonusRM,
                MS = championStats.MS + classBonus.BonusMS,
                Regen = championStats.Regen + (_selectedClass == "Mage" ? championStats.Regen * classBonus.BonusRegen : classBonus.BonusRegen),
                MagicPen = 0,
                Lethality = 0,
                Tenacity = 0,
                HealShieldPower = 0,
                LifeSteal = 0,
                AbilityHaste = 0
            };

            foreach (var item in _selectedItems)
            {
                foreach (var stat in item.Stats)
                {
                    var valueStr = stat.Value.Replace("+", "").Replace("%", "");
                    
                    if (double.TryParse(valueStr, out var value))
                    {
                        switch (stat.Name)
                        {
                            case "Points de vie":
                                finalStats.PV += (int)value;
                                break;
                            case "Dégâts d'attaque":
                                finalStats.AD += (int)value;
                                break;
                            case "Puissance magique":
                                finalStats.AP += (int)value;
                                break;
                            case "Vitesse d'attaque":
                                if (stat.Value.Contains("%"))
                                    finalStats.AS += value / 100.0;
                                else
                                    finalStats.AS += value;
                                break;
                            case "Armure":
                                finalStats.Armor += (int)value;
                                break;
                            case "Résistance magique":
                                finalStats.RM += (int)value;
                                break;
                            case "Vitesse de déplacement":
                                if (stat.Value.Contains("%"))
                                    finalStats.MS += (int)(finalStats.MS * (value / 100.0));
                                else
                                    finalStats.MS += (int)value;
                                break;
                            case "Régénération PV":
                                finalStats.Regen += value;
                                break;
                            case "Pénétration magique":
                                finalStats.MagicPen += (int)value;
                                break;
                            case "Léthалité":
                            case "Pénétration d'armure":
                                finalStats.Lethality += (int)value;
                                break;
                            case "Ténacité":
                                finalStats.Tenacity += value;
                                break;
                            case "Puissance soins/boucliers":
                                finalStats.HealShieldPower += value;
                                break;
                            case "Vol de vie":
                            case "Omnivampirisme":
                                finalStats.LifeSteal += value;
                                break;
                            case "Accélération":
                            case "Réduction de cooldown":
                                finalStats.AbilityHaste += (int)value;
                                break;
                        }
                    }
                }
            }

            return finalStats;
        }

        private void OnViewSummaryClick(object? sender, RoutedEventArgs e)
        {
            var finalStats = CalculateFinalStats();

            SummaryPlayerName.Text = _playerPseudo;

            try
            {
                SummaryChampion3D.Source = new Bitmap($"Assets/{_selectedChampion}3d.png");
            }
            catch
            {
                try { SummaryChampion3D.Source = new Bitmap($"Assets/{_selectedChampion}.jpg"); } catch { }
            }

            if (_championStats.TryGetValue(_selectedChampion, out var championStats))
            {
                SummaryAbilityName.Text = championStats.AbilityName;
                SummaryAbilityType.Text = championStats.AbilityType;
                SummaryAbilityDescription.Text = championStats.AbilityDescription;
            }

            SummaryStatPV.Text = finalStats.PV.ToString();
            SummaryStatAD.Text = finalStats.AD.ToString();
            SummaryStatAP.Text = finalStats.AP.ToString();
            SummaryStatAS.Text = finalStats.AS.ToString("0.00");
            SummaryStatArmor.Text = finalStats.Armor.ToString();
            SummaryStatRM.Text = finalStats.RM.ToString();
            SummaryStatMS.Text = finalStats.MS.ToString();
            SummaryStatRegen.Text = finalStats.Regen.ToString("0.0");

            SummaryStatMagicPen.Text = finalStats.MagicPen > 0 ? finalStats.MagicPen.ToString() : "0";
            SummaryStatLethality.Text = finalStats.Lethality > 0 ? finalStats.Lethality.ToString() : "0";
            SummaryStatTenacity.Text = finalStats.Tenacity > 0 ? $"{finalStats.Tenacity}%" : "0%";
            SummaryStatHealShieldPower.Text = finalStats.HealShieldPower > 0 ? $"{finalStats.HealShieldPower}%" : "0%";
            SummaryStatLifesteal.Text = finalStats.LifeSteal > 0 ? $"{finalStats.LifeSteal}%" : "0%";
            SummaryStatAbilityHaste.Text = finalStats.AbilityHaste > 0 ? finalStats.AbilityHaste.ToString() : "0";

            SummaryItemsGrid.Children.Clear();
            for (int i = 0; i < _selectedItems.Count && i < 6; i++)
            {
                var item = _selectedItems[i];
                
                var itemBorder = new Border
                {
                    Width = 80,
                    Height = 80,
                    Margin = new Avalonia.Thickness(10),
                    CornerRadius = new Avalonia.CornerRadius(8),
                    BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0f3460")),
                    BorderThickness = new Avalonia.Thickness(2),
                    Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#16213e")),
                    Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
                };

                var image = new Image
                {
                    Stretch = Avalonia.Media.Stretch.Fill
                };

                itemBorder.Child = image;

                var tooltipContent = CreateItemTooltip(item);
                ToolTip.SetTip(itemBorder, tooltipContent);

                int index = i;
                Task.Run(async () =>
                {
                    try
                    {
                        var imageBytes = await _httpClient.GetByteArrayAsync(item.ImageUrl);
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            using var stream = new System.IO.MemoryStream(imageBytes);
                            image.Source = new Bitmap(stream);
                        });
                    }
                    catch { }
                });

                Grid.SetRow(itemBorder, i / 3);
                Grid.SetColumn(itemBorder, i % 3);
                SummaryItemsGrid.Children.Add(itemBorder);
            }

            Page4.IsVisible = false;
            Page5.IsVisible = true;
        }

        private Border CreateItemTooltip(ItemData item)
        {
            var tooltipBorder = new Border
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#16213e")),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0f3460")),
                BorderThickness = new Avalonia.Thickness(2),
                CornerRadius = new Avalonia.CornerRadius(8),
                Padding = new Avalonia.Thickness(15),
                MaxWidth = 300
            };

            var tooltipStack = new StackPanel { Spacing = 8 };

            var nameText = new TextBlock
            {
                Text = item.Name,
                FontSize = 16,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#e94560")),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            };
            tooltipStack.Children.Add(nameText);

            var infoText = new TextBlock
            {
                Text = $"{item.Rarity} • {item.Gold}g",
                FontSize = 13,
                Foreground = Avalonia.Media.Brushes.LightGray
            };
            tooltipStack.Children.Add(infoText);

            var separator = new Border
            {
                Height = 1,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0f3460")),
                Margin = new Avalonia.Thickness(0, 5, 0, 5)
            };
            tooltipStack.Children.Add(separator);

            if (item.Stats.Any())
            {
                foreach (var stat in item.Stats)
                {
                    var statStack = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8
                    };

                    if (!string.IsNullOrEmpty(stat.IconPath))
                    {
                        try
                        {
                            var statIcon = new Image
                            {
                                Source = new Bitmap(stat.IconPath),
                                Width = 18,
                                Height = 18,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            statStack.Children.Add(statIcon);
                        }
                        catch { }
                    }

                    var statText = new TextBlock
                    {
                        Text = $"{stat.Value} {stat.Name}",
                        FontSize = 13,
                        Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#4ecca3")),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    statStack.Children.Add(statText);
                    tooltipStack.Children.Add(statStack);
                }
            }
            else
            {
                var noStatsText = new TextBlock
                {
                    Text = "Aucune statistique",
                    FontSize = 12,
                    Foreground = Avalonia.Media.Brushes.Gray,
                    FontStyle = Avalonia.Media.FontStyle.Italic
                };
                tooltipStack.Children.Add(noStatsText);
            }

            tooltipBorder.Child = tooltipStack;
            return tooltipBorder;
        }

        private void OnRestartClick(object? sender, RoutedEventArgs e)
        {
            _playerPseudo = string.Empty;
            _selectedChampion = string.Empty;
            _selectedRace = string.Empty;
            _selectedClass = string.Empty;
            _selectedItems.Clear();
            
            if (_lastSelectedChampionButton != null)
            {
                _lastSelectedChampionButton.Background = Avalonia.Media.Brushes.Transparent;
                _lastSelectedChampionButton = null;
            }
            
            if (_lastSelectedClassButton != null)
            {
                _lastSelectedClassButton.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#16213e"));
                _lastSelectedClassButton = null;
            }
            
            PseudoInput.Text = string.Empty;
            ContinueButton.IsEnabled = false;
            
            Page5.IsVisible = false;
            Page4.IsVisible = false;
            Page3.IsVisible = false;
            Page2.IsVisible = false;
            Page1.IsVisible = true;
        }

        private async Task LoadRandomItems()
        {
            try
            {
                ItemsLoadingPanel.IsVisible = true;
                ItemsDisplayPanel.IsVisible = false;

                var cts = new System.Threading.CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10));

                string json;
                try
                {
                    json = await _httpClient.GetStringAsync(ITEM_JSON_URL, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    throw new Exception("Timeout lors de la connexion à l'API League of Legends");
                }

                var itemsData = JsonSerializer.Deserialize<ItemsRoot>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (itemsData?.Data == null || !itemsData.Data.Any())
                {
                    throw new Exception("Aucune donnée d'items reçue de l'API");
                }

                var validItems = itemsData.Data
                    .Where(kvp => IsValidItem(kvp.Value))
                    .Select(kvp => ParseItemData(kvp.Key, kvp.Value))
                    .ToList();

                if (!validItems.Any())
                {
                    throw new Exception("Aucun item valide trouvé après filtrage");
                }

                var uniqueItems = validItems
                    .GroupBy(item => item.Name)
                    .Select(group => group.First())
                    .ToList();
                

                var boots = uniqueItems.Where(i => _bootsIds.Contains(i.Id)).ToList();
                var otherItems = uniqueItems.Where(i => !_bootsIds.Contains(i.Id)).ToList();

                _selectedItems.Clear();
                var random = new Random();

                if (boots.Any())
                {
                    _selectedItems.Add(boots[random.Next(boots.Count)]);
                }


                int attempts = 0;
                while (_selectedItems.Count < 6 && attempts < 50)
                {
                    attempts++;
                    var selectedRarity = SelectRarityByProbability(random);
                    var itemsOfRarity = otherItems
                        .Where(item => item.Rarity == selectedRarity && !_selectedItems.Contains(item))
                        .ToList();

                    if (itemsOfRarity.Any())
                    {
                        _selectedItems.Add(itemsOfRarity[random.Next(itemsOfRarity.Count)]);
                    }
                    else
                    {
                        var availableItems = otherItems.Where(item => !_selectedItems.Contains(item)).ToList();
                        if (availableItems.Any())
                        {
                            _selectedItems.Add(availableItems[random.Next(availableItems.Count)]);
                        }
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() => DisplayItems());
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Erreur HTTP chargement items: {httpEx.Message}");
                await Dispatcher.UIThread.InvokeAsync(() => ShowError($"Erreur de connexion à l'API: {httpEx.Message}\n\nVérifiez votre connexion internet."));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement items: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Dispatcher.UIThread.InvokeAsync(() => ShowError($"Erreur: {ex.Message}"));
            }
        }

        private void ShowError(string message)
        {
            ItemsLoadingPanel.IsVisible = false;
            ItemsContainer.Children.Clear();
            
            var errorPanel = new StackPanel
            {
                Spacing = 20,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(50)
            };

            var errorIcon = new TextBlock
            {
                Text = "❌",
                FontSize = 48,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var errorText = new TextBlock
            {
                Text = message,
                Foreground = Avalonia.Media.Brushes.Red,
                FontSize = 16,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 600
            };

            var retryButton = new Button
            {
                Content = "Réessayer",
                FontSize = 16,
                Padding = new Avalonia.Thickness(20, 10),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#e94560")),
                Foreground = Avalonia.Media.Brushes.White,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            };
            
            retryButton.Click += async (s, e) => await LoadRandomItems();

            errorPanel.Children.Add(errorIcon);
            errorPanel.Children.Add(errorText);
            errorPanel.Children.Add(retryButton);
            
            ItemsContainer.Children.Add(errorPanel);
            ItemsDisplayPanel.IsVisible = true;
        }

        private bool IsValidItem(ItemJson item)
        {
            if (item.Maps == null || !item.Maps.TryGetValue("11", out var isAvailable) || !isAvailable)
                return false;

            if (item.Gold == null || item.Gold.Total <= 0 || item.Gold.Purchasable != true)
                return false;

            return true;
        }

        private ItemData ParseItemData(string id, ItemJson itemJson)
        {
            var itemData = new ItemData
            {
                Id = id,
                Name = itemJson.Name ?? "Item inconnu",
                Description = itemJson.Description ?? "",
                Gold = itemJson.Gold?.Total ?? 0,
                Rarity = GetRarity(itemJson.Gold?.Total ?? 0),
                ImageUrl = $"{ITEM_IMAGE_BASE_URL}{id}.png"
            };
// Relie les icones.png aux stats
            var statMapping = new Dictionary<string, (string displayName, string iconPath)>
            {
                { "FlatHPPoolMod", ("Points de vie", "Assets/pv.png") },
                { "FlatPhysicalDamageMod", ("Dégâts d'attaque", "Assets/ad.png") },
                { "FlatMagicDamageMod", ("Puissance magique", "Assets/ap.png") },
                { "PercentAttackSpeedMod", ("Vitesse d'attaque", "Assets/as.png") },
                { "FlatArmorMod", ("Armure", "Assets/armure.png") },
                { "FlatSpellBlockMod", ("Résistance magique", "Assets/rm.png") },
                { "PercentMovementSpeedMod", ("Vitesse de déplacement", "Assets/ms.png") },
                { "FlatMovementSpeedMod", ("Vitesse de déplacement", "Assets/ms.png") },
                { "FlatHPRegenMod", ("Régénération PV", "Assets/regen.png") },
                { "PercentBaseHPRegenMod", ("Régénération PV", "Assets/regen.png") },
                { "FlatMPPoolMod", ("Mana", "Assets/ap.png") },
                { "FlatMPRegenMod", ("Régénération mana", "Assets/ap.png") },
                { "PercentBaseMPRegenMod", ("Régénération mana", "Assets/ap.png") },
                { "FlatCritChanceMod", ("Chances critiques", "Assets/crit.png") },
                { "PercentCritChanceMod", ("Chances critiques", "Assets/crit.png") },
                { "FlatCritDamageMod", ("Dégâts critiques", "Assets/crit.png") },
                { "PercentCritDamageMod", ("Dégâts critiques", "Assets/crit.png") },
                { "PercentLifeStealMod", ("Vol de vie", "Assets/volvie.png") },
                { "PercentSpellVampMod", ("Vampirisme magique", "Assets/volvie.png") },
                { "PercentOmnivampMod", ("Omnivampirisme", "Assets/volvie.png") },
                { "FlatArmorPenetrationMod", ("Pénétration d'armure", "Assets/letha.png") },
                { "FlatPhysicalDamageModPerLevel", ("Léthалité", "Assets/letha.png") },
                { "FlatMagicPenetrationMod", ("Pénétration magique", "Assets/penmag.png") },
                { "PercentArmorPenetrationMod", ("Pénétration d'armure", "Assets/letha.png") },
                { "PercentMagicPenetrationMod", ("Pénétration magique", "Assets/penmag.png") },
                { "PercentCooldownMod", ("Réduction de cooldown", "Assets/ability.png") },
                { "AbilityHasteMod", ("Accélération", "Assets/ability.png") },
                { "PercentTenacityMod", ("Ténacité", "Assets/tenacity.png") },
                { "FlatTenacityMod", ("Ténacité", "Assets/tenacity.png") },
                { "PercentHealAndShieldPower", ("Puissance soins/boucliers", "Assets/regenpower.png") },
                { "PercentBonusArmorPenetrationMod", ("Pénétration armure bonus", "Assets/letha.png") },
                { "PercentBonusMagicPenetrationMod", ("Pénétration mag. bonus", "Assets/penmag.png") }
            };

            var addedStats = new HashSet<string>();

            if (itemJson.Stats != null)
            {
                foreach (var stat in itemJson.Stats)
                {
                    if (statMapping.TryGetValue(stat.Key, out var mapping))
                    {
                        string valueStr;
                        if (stat.Key.StartsWith("Percent") || stat.Key.Contains("Percent"))
                        {
                            valueStr = $"+{(stat.Value * 100):0}%";
                        }
                        else
                        {
                            valueStr = $"+{stat.Value:0}";
                        }

                        var statKey = $"{mapping.displayName}_{valueStr}";
                        if (!addedStats.Contains(statKey))
                        {
                            itemData.Stats.Add(new ItemStat
                            {
                                Name = mapping.displayName,
                                Value = valueStr,
                                IconPath = mapping.iconPath
                            });
                            addedStats.Add(statKey);
                        }
                    }
                }
            }
// Ajout des stats des items aux stats de base
            if (!string.IsNullOrEmpty(itemJson.Description))
            {
                var desc = itemJson.Description;
                
                var armorPenMatch = System.Text.RegularExpressions.Regex.Match(desc, @"\+(\d+)%.*?pénétration d'armure");
                if (armorPenMatch.Success)
                {
                    var value = armorPenMatch.Groups[1].Value;
                    var statKey = $"Pénétration d'armure_+{value}%";
                    if (!addedStats.Contains(statKey))
                    {
                        itemData.Stats.Add(new ItemStat
                        {
                            Name = "Pénétration d'armure",
                            Value = $"+{value}%",
                            IconPath = "Assets/letha.png"
                        });
                        addedStats.Add(statKey);
                    }
                }
                
                var magicPenMatch = System.Text.RegularExpressions.Regex.Match(desc, @"\+(\d+)%?\s*pénétration magique");
                if (magicPenMatch.Success)
                {
                    var value = magicPenMatch.Groups[1].Value;
                    var isPercent = desc.Contains($"+{value}%");
                    var valueStr = isPercent ? $"+{value}%" : $"+{value}";
                    var statKey = $"Pénétration magique_{valueStr}";
                    if (!addedStats.Contains(statKey))
                    {
                        itemData.Stats.Add(new ItemStat
                        {
                            Name = "Pénétration magique",
                            Value = valueStr,
                            IconPath = "Assets/penmag.png"
                        });
                        addedStats.Add(statKey);
                    }
                }
                
                var lethalityMatch = System.Text.RegularExpressions.Regex.Match(desc, @"\+(\d+)\s*létalité");
                if (lethalityMatch.Success)
                {
                    var value = lethalityMatch.Groups[1].Value;
                    var statKey = $"Léthалité_+{value}";
                    if (!addedStats.Contains(statKey))
                    {
                        itemData.Stats.Add(new ItemStat
                        {
                            Name = "Léthалité",
                            Value = $"+{value}",
                            IconPath = "Assets/letha.png"
                        });
                        addedStats.Add(statKey);
                    }
                }
                
                var abilityHasteMatch = System.Text.RegularExpressions.Regex.Match(desc, @"\+(\d+)\s*accélération de compétence");
                if (abilityHasteMatch.Success)
                {
                    var value = abilityHasteMatch.Groups[1].Value;
                    var statKey = $"Accélération_+{value}";
                    if (!addedStats.Contains(statKey))
                    {
                        itemData.Stats.Add(new ItemStat
                        {
                            Name = "Accélération",
                            Value = $"+{value}",
                            IconPath = "Assets/ability.png"
                        });
                        addedStats.Add(statKey);
                    }
                }
                
                var healShieldMatch = System.Text.RegularExpressions.Regex.Match(desc, @"\+(\d+)%.*?efficacité des soins et boucliers");
                if (healShieldMatch.Success)
                {
                    var value = healShieldMatch.Groups[1].Value;
                    var statKey = $"Puissance soins/boucliers_+{value}%";
                    if (!addedStats.Contains(statKey))
                    {
                        itemData.Stats.Add(new ItemStat
                        {
                            Name = "Puissance soins/boucliers",
                            Value = $"+{value}%",
                            IconPath = "Assets/regenpower.png"
                        });
                        addedStats.Add(statKey);
                    }
                }
                
                var tenacityMatch = System.Text.RegularExpressions.Regex.Match(desc, @"\+(\d+)%.*?ténacité");
                if (tenacityMatch.Success)
                {
                    var value = tenacityMatch.Groups[1].Value;
                    var statKey = $"Ténacité_+{value}%";
                    if (!addedStats.Contains(statKey))
                    {
                        itemData.Stats.Add(new ItemStat
                        {
                            Name = "Ténacité",
                            Value = $"+{value}%",
                            IconPath = "Assets/tenacity.png"
                        });
                        addedStats.Add(statKey);
                    }
                }
            }

            return itemData;
        }

        private string GetRarity(int gold)
        {
            return gold switch
            {
                < 1000 => "Commun",
                <= 2000 => "Rare",
                <= 3000 => "Épique",
                _ => "Légendaire"
            };
        }

        private string SelectRarityByProbability(Random random)
        {
            var roll = random.Next(100);
            return roll switch
            {
                < 30 => "Commun",
                < 60 => "Rare",
                < 90 => "Épique",
                _ => "Légendaire"
            };
        }

        private async void DisplayItems()
        {
            ItemsLoadingPanel.IsVisible = false;
            ItemsContainer.Children.Clear();
            ItemsDisplayPanel.IsVisible = true;

            foreach (var item in _selectedItems)
            {
                Console.WriteLine($"Item: {item.Name} - Stats: {item.Stats.Count}");
                
                var itemCard = CreateItemCard(item);
                ItemsContainer.Children.Add(itemCard);
                
                if (item != _selectedItems.Last())
                {
                    await Task.Delay(1000);
                }
            }
        }

        private Border CreateItemCard(ItemData item)
        {
            var rarityColor = item.Rarity switch
            {
                "Commun" => "#808080",
                "Rare" => "#4169E1",
                "Épique" => "#9370DB",
                "Légendaire" => "#FFD700",
                _ => "#666666"
            };

            var card = new Border
            {
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#16213e")),
                BorderBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(rarityColor)),
                BorderThickness = new Avalonia.Thickness(3),
                CornerRadius = new Avalonia.CornerRadius(10),
                Padding = new Avalonia.Thickness(20),
                Margin = new Avalonia.Thickness(15),
                Width = 420
            };

            var mainStack = new StackPanel
            {
                Spacing = 12
            };

            var itemHeaderStack = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 15
            };

            var image = new Image
            {
                Width = 64,
                Height = 64,
                VerticalAlignment = VerticalAlignment.Center
            };

            Task.Run(async () =>
            {
                try
                {
                    var imageBytes = await _httpClient.GetByteArrayAsync(item.ImageUrl);
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        using var stream = new System.IO.MemoryStream(imageBytes);
                        image.Source = new Bitmap(stream);
                    });
                }
                catch { }
            });

            itemHeaderStack.Children.Add(image);

            var titleStack = new StackPanel
            {
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center
            };

            var nameText = new TextBlock
            {
                Text = item.Name,
                FontSize = 18,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse(rarityColor)),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 300
            };
            titleStack.Children.Add(nameText);

            var rarityText = new TextBlock
            {
                Text = $"{item.Rarity} • {item.Gold}g",
                FontSize = 14,
                Foreground = Avalonia.Media.Brushes.LightGray
            };
            titleStack.Children.Add(rarityText);

            itemHeaderStack.Children.Add(titleStack);
            mainStack.Children.Add(itemHeaderStack);

            var separator = new Border
            {
                Height = 2,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#0f3460")),
                Margin = new Avalonia.Thickness(0, 5, 0, 10)
            };
            mainStack.Children.Add(separator);

            if (!item.Stats.Any())
            {
                var descText = new TextBlock
                {
                    Text = string.IsNullOrEmpty(item.Description) ? "Aucune statistique disponible pour cet item." : 
                           System.Text.RegularExpressions.Regex.Replace(item.Description, "<.*?>", ""),
                    FontSize = 13,
                    Foreground = Avalonia.Media.Brushes.LightGray,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(0, 5, 0, 0),
                    FontStyle = Avalonia.Media.FontStyle.Italic
                };
                mainStack.Children.Add(descText);
            }

            if (item.Stats.Any())
            {
                var statsPanel = new StackPanel { Spacing = 8 };

                foreach (var stat in item.Stats)
                {
                    var statStack = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8
                    };

                    if (!string.IsNullOrEmpty(stat.IconPath))
                    {
                        try
                        {
                            var statIcon = new Image
                            {
                                Source = new Bitmap(stat.IconPath),
                                Width = 20,
                                Height = 20,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                            statStack.Children.Add(statIcon);
                        }
                        catch { }
                    }

                    var statText = new TextBlock
                    {
                        Text = $"{stat.Value} {stat.Name}",
                        FontSize = 14,
                        Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#4ecca3")),
                        FontWeight = Avalonia.Media.FontWeight.SemiBold,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    statStack.Children.Add(statText);
                    statsPanel.Children.Add(statStack);
                }

                mainStack.Children.Add(statsPanel);
            }

            card.Child = mainStack;
            return card;
        }

        private record ChampionStats(
            string Name,
            string Race,
            int PV,
            int AD,
            int AP,
            double AS,
            int Armor,
            int RM,
            int MS,
            double Regen,
            string AbilityName,
            string AbilityType,
            string AbilityDescription
        );
        
        private record ClassBonus(
            int BonusPV,
            int BonusAD,
            int BonusAP,
            double BonusAS,
            int BonusArmor,
            int BonusRM,
            int BonusMS,
            double BonusRegen
        );

        private class FinalStats
        {
            public int PV { get; set; }
            public int AD { get; set; }
            public int AP { get; set; }
            public double AS { get; set; }
            public int Armor { get; set; }
            public int RM { get; set; }
            public int MS { get; set; }
            public double Regen { get; set; }
            public int MagicPen { get; set; }
            public int Lethality { get; set; }
            public double Tenacity { get; set; }
            public double HealShieldPower { get; set; }
            public double LifeSteal { get; set; }
            public int AbilityHaste { get; set; }
        }

        private class ItemData
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public int Gold { get; set; }
            public string Rarity { get; set; } = "";
            public string ImageUrl { get; set; } = "";
            public List<ItemStat> Stats { get; set; } = new();
        }

        private class ItemStat
        {
            public string Name { get; set; } = "";
            public string Value { get; set; } = "";
            public string? IconPath { get; set; }
        }

        private class ItemsRoot
        {
            public Dictionary<string, ItemJson>? Data { get; set; }
        }

        private class ItemJson
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? Plaintext { get; set; }
            public ItemGold? Gold { get; set; }
            public Dictionary<string, bool>? Maps { get; set; }
            public Dictionary<string, double>? Stats { get; set; }
        }

        private class ItemGold
        {
            public int Total { get; set; }
            public bool Purchasable { get; set; }
        }
    }
}