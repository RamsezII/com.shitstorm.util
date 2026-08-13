#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _UTIL_.Editor
{
    internal sealed class GitWatchLinuxWindow : EditorWindow
    {
        const float TableWidth = 1335f;
        const float RowHeight = 54f;

        readonly ConcurrentQueue<string> progressMessages = new ConcurrentQueue<string>();
        readonly List<GitWatchRepositoryState> repositories = new List<GitWatchRepositoryState>();

        CancellationTokenSource cancellation;
        Task<GitWatchScanResult> activeTask;
        Vector2 tableScroll;
        string root = string.Empty;
        string search = string.Empty;
        string status = "Préparation…";
        GitWatchRepositoryState selectedRepository;
        bool stylesReady;

        GUIStyle titleStyle;
        GUIStyle subtitleStyle;
        GUIStyle summaryLabelStyle;
        GUIStyle summaryValueStyle;
        GUIStyle tableHeaderStyle;
        GUIStyle cellStyle;
        GUIStyle mutedCellStyle;
        GUIStyle commitStyle;
        GUIStyle footerStyle;

        internal static void Open(string projectRoot)
        {
            GitWatchLinuxWindow window = GetWindow<GitWatchLinuxWindow>();
            window.titleContent = new GUIContent("Git Watch");
            window.minSize = new Vector2(960f, 560f);
            window.root = Path.GetFullPath(projectRoot);
            window.Show();
            window.Focus();
            window.StartOperation(GitWatchMode.Status);
        }

        void OnEnable()
        {
            if (string.IsNullOrWhiteSpace(root))
                root = Directory.GetParent(Application.dataPath)!.FullName;
            EditorApplication.update += PollWorker;
        }

        void OnDisable()
        {
            EditorApplication.update -= PollWorker;
            cancellation?.Cancel();
        }

        void PollWorker()
        {
            while (progressMessages.TryDequeue(out string message))
                status = message;

            if (activeTask == null)
                return;

            if (!activeTask.IsCompleted)
            {
                Repaint();
                return;
            }

            try
            {
                GitWatchScanResult result = activeTask.GetAwaiter().GetResult();
                repositories.Clear();
                repositories.AddRange(result.Repositories.OrderBy(repository => repository.Name, StringComparer.OrdinalIgnoreCase));
                status = result.Status;
            }
            catch (OperationCanceledException)
            {
                status = "Opération annulée.";
            }
            catch (Exception exception)
            {
                status = $"Erreur · {exception.Message}";
                Debug.LogError($"Git Watch Linux :\n{exception}");
            }
            finally
            {
                activeTask = null;
                cancellation?.Dispose();
                cancellation = null;
                Repaint();
            }
        }

        void StartOperation(GitWatchMode mode)
        {
            if (activeTask != null || string.IsNullOrWhiteSpace(root))
                return;

            cancellation = new CancellationTokenSource();
            repositories.Clear();
            selectedRepository = null;
            status = mode == GitWatchMode.Fetch
                ? "Actualisation des remotes…"
                : mode == GitWatchMode.PullSafe
                    ? "Mise à jour prudente…"
                    : mode == GitWatchMode.DiscardSuspicious
                        ? "Restauration des atlas TMP…"
                        : "Lecture des états locaux…";

            CancellationToken token = cancellation.Token;
            activeTask = Task.Run(
                () => GitWatchLinuxEngine.Run(root, mode, message => progressMessages.Enqueue(message), token),
                token);
        }

        void OnGUI()
        {
            EnsureStyles();
            DrawHeader();
            GUILayout.Space(14f);
            DrawSummary();
            GUILayout.Space(14f);
            DrawSearch();
            GUILayout.Space(6f);
            DrawTable();
            DrawFooter();
        }

        void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("SHITSTORM  /  GIT WATCH", titleStyle);
            EditorGUILayout.LabelField(root, subtitleStyle);
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUI.enabled = activeTask == null;

            int suspiciousCount = repositories.Sum(repository => repository.SuspiciousChanges.Count);
            if (suspiciousCount > 0 && GUILayout.Button($"↶  Discard TMP ({suspiciousCount})", GUILayout.Height(34f)))
                ConfirmDiscard();

            if (GUILayout.Button("Vérifier", GUILayout.Height(34f)))
                StartOperation(GitWatchMode.Status);
            if (GUILayout.Button("↻  Fetch global", GUILayout.Height(34f)))
                StartOperation(GitWatchMode.Fetch);
            if (GUILayout.Button("↓  Mettre à jour", GUILayout.Height(34f)))
                StartOperation(GitWatchMode.PullSafe);

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        void DrawSummary()
        {
            int clean = repositories.Count(repository => repository.StatusKey == GitWatchStatus.Clean);
            int modified = repositories.Count(repository => repository.DirtyCount > 0);
            int behind = repositories.Count(repository => repository.Behind > 0);
            int attention = repositories.Count(repository =>
                repository.StatusKey == GitWatchStatus.Diverged ||
                repository.StatusKey == GitWatchStatus.Error ||
                repository.StatusKey == GitWatchStatus.NoUpstream ||
                repository.SuspiciousChanges.Count > 0);

            EditorGUILayout.BeginHorizontal();
            DrawSummaryCard("DÉPÔTS", repositories.Count, Color.white);
            DrawSummaryCard("À JOUR", clean, HtmlColor("#61D6A3"));
            DrawSummaryCard("MODIFIÉS", modified, HtmlColor("#EAC66B"));
            DrawSummaryCard("EN RETARD", behind, HtmlColor("#FFB45E"));
            DrawSummaryCard("À VÉRIFIER", attention, HtmlColor("#FF6B7A"));
            EditorGUILayout.EndHorizontal();
        }

        void DrawSummaryCard(string label, int value, Color color)
        {
            Rect card = EditorGUILayout.GetControlRect(false, 68f, GUILayout.ExpandWidth(true));
            card.xMax -= 6f;
            EditorGUI.DrawRect(card, HtmlColor("#171D29"));

            Rect labelRect = new Rect(card.x + 12f, card.y + 9f, card.width - 24f, 18f);
            Rect valueRect = new Rect(card.x + 12f, card.y + 27f, card.width - 24f, 32f);
            GUI.Label(labelRect, label, summaryLabelStyle);
            Color previous = GUI.color;
            GUI.color = color;
            GUI.Label(valueRect, value.ToString(), summaryValueStyle);
            GUI.color = previous;
        }

        void DrawSearch()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ÉTAT DES DÉPÔTS", summaryLabelStyle);
            GUILayout.FlexibleSpace();
            search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(270f));
            EditorGUILayout.EndHorizontal();
        }

        void DrawTable()
        {
            tableScroll = EditorGUILayout.BeginScrollView(tableScroll);
            DrawTableHeader();

            IEnumerable<GitWatchRepositoryState> visibleRepositories = repositories;
            if (!string.IsNullOrWhiteSpace(search))
            {
                visibleRepositories = visibleRepositories.Where(repository =>
                    Contains(repository.Name, search) ||
                    Contains(repository.Branch, search) ||
                    Contains(repository.Author, search) ||
                    Contains(repository.StatusText, search));
            }

            int rowIndex = 0;
            foreach (GitWatchRepositoryState repository in visibleRepositories)
                DrawRepositoryRow(repository, rowIndex++);

            if (repositories.Count == 0 && activeTask == null)
            {
                Rect emptyRect = GUILayoutUtility.GetRect(TableWidth, 80f);
                GUI.Label(emptyRect, "Aucun dépôt Git détecté.", mutedCellStyle);
            }

            EditorGUILayout.EndScrollView();
        }

        void DrawTableHeader()
        {
            Rect row = GUILayoutUtility.GetRect(TableWidth, 31f);
            EditorGUI.DrawRect(row, HtmlColor("#10151E"));
            float x = row.x;
            DrawHeaderCell(ref x, row.y, 155f, "DÉPÔT");
            DrawHeaderCell(ref x, row.y, 145f, "BRANCHE");
            DrawHeaderCell(ref x, row.y, 160f, "CHANGEMENTS LOCAUX");
            DrawHeaderCell(ref x, row.y, 175f, "SYNCHRO");
            DrawHeaderCell(ref x, row.y, 330f, "DERNIER COMMIT");
            DrawHeaderCell(ref x, row.y, 125f, "AUTEUR");
            DrawHeaderCell(ref x, row.y, 245f, "ACTION / DIAGNOSTIC");
        }

        void DrawHeaderCell(ref float x, float y, float width, string text)
        {
            GUI.Label(new Rect(x + 10f, y, width - 12f, 31f), text, tableHeaderStyle);
            x += width;
        }

        void DrawRepositoryRow(GitWatchRepositoryState repository, int rowIndex)
        {
            Rect row = GUILayoutUtility.GetRect(TableWidth, RowHeight);
            Color background = selectedRepository == repository
                ? HtmlColor("#202D48")
                : HtmlColor(rowIndex % 2 == 0 ? "#121722" : "#141A25");
            EditorGUI.DrawRect(row, background);
            EditorGUI.DrawRect(new Rect(row.x, row.yMax - 1f, row.width, 1f), HtmlColor("#202736"));

            string tooltip = BuildTooltip(repository);
            float x = row.x;
            DrawCell(ref x, row.y, 155f, repository.Name, tooltip, cellStyle);
            DrawCell(ref x, row.y, 145f, repository.Branch, tooltip, cellStyle);
            DrawBadge(ref x, row.y, 160f, repository.LocalText, LocalColor(repository));
            DrawBadge(ref x, row.y, 175f, repository.StatusText, StatusColor(repository.StatusKey));

            Rect commitRect = new Rect(x + 10f, row.y + 6f, 310f, RowHeight - 8f);
            GUI.Label(commitRect, new GUIContent($"{repository.Subject}\n{repository.LastCommit}", tooltip), commitStyle);
            x += 330f;
            DrawCell(ref x, row.y, 125f, repository.Author, tooltip, cellStyle);

            string operation = repository.Operation;
            if (repository.SuspiciousChanges.Count > 0)
            {
                string diagnostic = repository.SuspiciousChanges.Count == 1
                    ? "1 atlas TMP suspect"
                    : $"{repository.SuspiciousChanges.Count} atlas TMP suspects";
                operation = string.IsNullOrWhiteSpace(operation) ? diagnostic : $"{operation} · {diagnostic}";
            }
            DrawCell(ref x, row.y, 245f, string.IsNullOrWhiteSpace(operation) ? "—" : operation, tooltip, mutedCellStyle);

            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && row.Contains(currentEvent.mousePosition))
            {
                selectedRepository = repository;
                if (currentEvent.clickCount == 2)
                    EditorUtility.RevealInFinder(repository.Path);
                currentEvent.Use();
                Repaint();
            }
        }

        void DrawCell(ref float x, float y, float width, string text, string tooltip, GUIStyle style)
        {
            GUI.Label(new Rect(x + 10f, y + 2f, width - 14f, RowHeight - 4f), new GUIContent(text, tooltip), style);
            x += width;
        }

        void DrawBadge(ref float x, float y, float width, string text, Color color)
        {
            Rect badge = new Rect(x + 9f, y + 14f, width - 18f, 26f);
            Color badgeBackground = new Color(color.r * 0.22f, color.g * 0.22f, color.b * 0.22f, 1f);
            EditorGUI.DrawRect(badge, badgeBackground);
            Color previous = GUI.color;
            GUI.color = color;
            GUI.Label(new Rect(badge.x + 8f, badge.y, badge.width - 16f, badge.height), text, cellStyle);
            GUI.color = previous;
            x += width;
        }

        void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (activeTask != null)
                GUILayout.Label("●", GUILayout.Width(18f));
            GUILayout.Label(status, footerStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Double-clique un dépôt pour l’ouvrir", footerStyle);
            EditorGUILayout.EndHorizontal();
        }

        void ConfirmDiscard()
        {
            List<string> targets = repositories
                .SelectMany(repository => repository.SuspiciousChanges.Select(change => $"• {repository.Name} / {change.Path}"))
                .ToList();

            string message =
                "Git Watch va restaurer ces fichiers exactement comme dans le dernier commit :\n\n" +
                string.Join("\n", targets) +
                "\n\nToutes leurs modifications locales seront supprimées, y compris si elles sont stagées.\n" +
                "Cette action ne touche à aucun autre fichier.";

            if (EditorUtility.DisplayDialog("Discard des atlas TMP", message, "Restaurer", "Annuler"))
                StartOperation(GitWatchMode.DiscardSuspicious);
        }

        void EnsureStyles()
        {
            if (stylesReady)
                return;

            titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 20 };
            titleStyle.normal.textColor = HtmlColor("#EDF1F7");
            subtitleStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = 11 };
            subtitleStyle.normal.textColor = HtmlColor("#6F7989");
            summaryLabelStyle = new GUIStyle(EditorStyles.miniBoldLabel) { fontSize = 10 };
            summaryLabelStyle.normal.textColor = HtmlColor("#7F8A9C");
            summaryValueStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 23 };
            tableHeaderStyle = new GUIStyle(EditorStyles.miniBoldLabel) { alignment = TextAnchor.MiddleLeft };
            tableHeaderStyle.normal.textColor = HtmlColor("#7F8A9C");
            cellStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft, clipping = TextClipping.Clip, fontSize = 12 };
            cellStyle.normal.textColor = HtmlColor("#E8EDF6");
            mutedCellStyle = new GUIStyle(cellStyle) { wordWrap = true };
            mutedCellStyle.normal.textColor = HtmlColor("#8E99AA");
            commitStyle = new GUIStyle(cellStyle) { wordWrap = false, richText = false, clipping = TextClipping.Clip };
            commitStyle.normal.textColor = HtmlColor("#DEE4EE");
            footerStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleLeft };
            footerStyle.normal.textColor = HtmlColor("#818C9E");
            stylesReady = true;
        }

        static string BuildTooltip(GitWatchRepositoryState repository)
        {
            StringBuilder tooltip = new StringBuilder(repository.Path);
            if (!string.IsNullOrWhiteSpace(repository.Remote))
                tooltip.AppendLine().Append(repository.Remote);
            if (!string.IsNullOrWhiteSpace(repository.Error))
                tooltip.AppendLine().Append(repository.Error);

            if (repository.SuspiciousChanges.Count > 0)
            {
                tooltip.AppendLine().AppendLine().Append("CHANGEMENTS SUSPECTS");
                foreach (GitWatchSuspiciousChange change in repository.SuspiciousChanges)
                    tooltip.AppendLine().Append("• ").Append(change.Path).AppendLine().Append("  ").Append(change.Reason);
            }

            return tooltip.ToString();
        }

        static bool Contains(string value, string term) =>
            !string.IsNullOrEmpty(value) && value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        static Color LocalColor(GitWatchRepositoryState repository)
        {
            if (!string.IsNullOrWhiteSpace(repository.Error))
                return HtmlColor("#FF6B7A");
            return repository.DirtyCount == 0 ? HtmlColor("#61D6A3") : HtmlColor("#EAC66B");
        }

        static Color StatusColor(GitWatchStatus statusKey)
        {
            switch (statusKey)
            {
                case GitWatchStatus.Behind: return HtmlColor("#FFB45E");
                case GitWatchStatus.Ahead: return HtmlColor("#66A6FF");
                case GitWatchStatus.Diverged:
                case GitWatchStatus.Error: return HtmlColor("#FF6B7A");
                case GitWatchStatus.NoUpstream: return HtmlColor("#AAB2C2");
                default: return HtmlColor("#61D6A3");
            }
        }

        static Color HtmlColor(string html)
        {
            return ColorUtility.TryParseHtmlString(html, out Color color) ? color : Color.white;
        }
    }

    internal enum GitWatchMode
    {
        Status,
        Fetch,
        PullSafe,
        DiscardSuspicious
    }

    internal enum GitWatchStatus
    {
        Clean,
        Modified,
        Ahead,
        Behind,
        Diverged,
        NoUpstream,
        Error
    }

    internal sealed class GitWatchScanResult
    {
        internal readonly List<GitWatchRepositoryState> Repositories = new List<GitWatchRepositoryState>();
        internal string Status = string.Empty;
    }

    internal sealed class GitWatchRepositoryState
    {
        internal string Name = string.Empty;
        internal string Path = string.Empty;
        internal string Branch = string.Empty;
        internal int DirtyCount;
        internal string LocalText = string.Empty;
        internal int Ahead;
        internal int Behind;
        internal bool HasUpstream;
        internal GitWatchStatus StatusKey;
        internal string StatusText = string.Empty;
        internal string Author = "—";
        internal string LastCommit = "Aucun commit";
        internal string Subject = string.Empty;
        internal string Remote = string.Empty;
        internal string Operation = string.Empty;
        internal string Error = string.Empty;
        internal int RestoredCount;
        internal readonly List<string> ChangedFiles = new List<string>();
        internal readonly List<GitWatchSuspiciousChange> SuspiciousChanges = new List<GitWatchSuspiciousChange>();
    }

    internal sealed class GitWatchSuspiciousChange
    {
        internal string Path = string.Empty;
        internal string Reason = string.Empty;
    }

    internal static class GitWatchLinuxEngine
    {
        static readonly HashSet<string> IgnoredDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".git", ".vs", ".idea", ".cache", "Library", "Temp", "Logs", "obj",
            "Build", "Builds", "MemoryCaptures", "Recordings", "node_modules"
        };

        internal static GitWatchScanResult Run(
            string root,
            GitWatchMode mode,
            Action<string> reportProgress,
            CancellationToken cancellationToken)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            List<string> repositoryPaths = FindGitRepositories(root, cancellationToken);
            GitWatchScanResult result = new GitWatchScanResult();
            reportProgress(repositoryPaths.Count == 1 ? "1 dépôt détecté" : $"{repositoryPaths.Count} dépôts détectés");

            for (int index = 0; index < repositoryPaths.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string repositoryPath = repositoryPaths[index];
                string repositoryName = DisplayName(repositoryPath);
                string operation = string.Empty;
                reportProgress($"Analyse {index + 1}/{repositoryPaths.Count} · {repositoryName}");

                if (mode == GitWatchMode.Fetch || mode == GitWatchMode.PullSafe)
                {
                    reportProgress($"Fetch {index + 1}/{repositoryPaths.Count} · {repositoryName}");
                    GitResult fetch = InvokeGit(repositoryPath, new[] { "fetch", "--all", "--prune", "--quiet" }, 180, cancellationToken);
                    operation = fetch.Success ? "Fetch effectué" : $"Fetch impossible : {fetch.Error}";
                }

                GitWatchRepositoryState state = GetRepositoryState(repositoryPath, operation, cancellationToken);

                if (mode == GitWatchMode.PullSafe && string.IsNullOrWhiteSpace(state.Error))
                    PullSafely(state, cancellationToken);

                if (mode == GitWatchMode.DiscardSuspicious &&
                    string.IsNullOrWhiteSpace(state.Error) &&
                    state.SuspiciousChanges.Count > 0)
                {
                    RestoreSuspiciousFiles(state, cancellationToken);
                }

                result.Repositories.Add(state);
            }

            stopwatch.Stop();
            int failures = result.Repositories.Count(repository => repository.Operation.IndexOf("impossible", StringComparison.OrdinalIgnoreCase) >= 0);
            int restored = result.Repositories.Sum(repository => repository.RestoredCount);
            int updated = result.Repositories.Count(repository => repository.Operation.StartsWith("Mis à jour", StringComparison.Ordinal));
            int skipped = result.Repositories.Count(repository => repository.Operation.StartsWith("Ignoré", StringComparison.Ordinal));
            string duration = (stopwatch.ElapsedMilliseconds / 1000d).ToString("0.0");

            switch (mode)
            {
                case GitWatchMode.Fetch:
                    result.Status = $"Remotes actualisés · {result.Repositories.Count} dépôt(s) · {duration} s" +
                                    (failures > 0 ? $" · {failures} échec(s)" : string.Empty);
                    break;
                case GitWatchMode.PullSafe:
                    result.Status = $"Mise à jour terminée · {updated} dépôt(s) mis à jour · {skipped} protégé(s) · {duration} s";
                    break;
                case GitWatchMode.DiscardSuspicious:
                    result.Status = $"Restauration terminée · {restored} atlas TMP restauré(s) · {duration} s";
                    break;
                default:
                    result.Status = $"État local actualisé · {result.Repositories.Count} dépôt(s) · {duration} s";
                    break;
            }

            return result;
        }

        static List<string> FindGitRepositories(string searchRoot, CancellationToken cancellationToken)
        {
            string fullRoot = Path.GetFullPath(searchRoot);
            Stack<string> pending = new Stack<string>();
            HashSet<string> found = new HashSet<string>(StringComparer.Ordinal);
            pending.Push(fullRoot);

            while (pending.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string directory = pending.Pop();
                if (Directory.Exists(Path.Combine(directory, ".git")) || File.Exists(Path.Combine(directory, ".git")))
                    found.Add(directory);

                try
                {
                    foreach (string child in Directory.EnumerateDirectories(directory))
                    {
                        string name = Path.GetFileName(child);
                        if (IgnoredDirectories.Contains(name))
                            continue;
                        if ((File.GetAttributes(child) & FileAttributes.ReparsePoint) != 0)
                            continue;
                        pending.Push(child);
                    }
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }

            return found
                .OrderBy(path => path == fullRoot ? "0" : "1" + path, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        static GitWatchRepositoryState GetRepositoryState(
            string repositoryPath,
            string operation,
            CancellationToken cancellationToken)
        {
            GitWatchRepositoryState state = new GitWatchRepositoryState
            {
                Name = DisplayName(repositoryPath),
                Path = repositoryPath,
                Operation = operation
            };

            GitResult branchResult = InvokeGit(repositoryPath, new[] { "branch", "--show-current" }, 20, cancellationToken);
            if (!branchResult.Success)
            {
                state.Branch = "—";
                state.LocalText = "Inaccessible";
                state.StatusText = "Erreur Git";
                state.StatusKey = GitWatchStatus.Error;
                state.Subject = branchResult.Error;
                state.Error = branchResult.Error;
                return state;
            }

            state.Branch = branchResult.Output.Trim();
            if (string.IsNullOrWhiteSpace(state.Branch))
            {
                GitResult head = InvokeGit(repositoryPath, new[] { "rev-parse", "--short", "HEAD" }, 20, cancellationToken);
                state.Branch = head.Success ? $"HEAD détachée · {head.Output.Trim()}" : "Dépôt vide";
            }

            GitResult status = InvokeGit(repositoryPath, new[] { "status", "--porcelain", "--untracked-files=normal" }, 60, cancellationToken);
            if (status.Success && !string.IsNullOrWhiteSpace(status.Output))
            {
                state.ChangedFiles.AddRange(GetChangedPaths(repositoryPath, cancellationToken));
                state.SuspiciousChanges.AddRange(FindSuspiciousChanges(repositoryPath, state.ChangedFiles, cancellationToken));
            }
            state.DirtyCount = state.ChangedFiles.Count;
            state.LocalText = !status.Success
                ? "État inconnu"
                : state.DirtyCount == 0
                    ? "Propre"
                    : state.DirtyCount == 1 ? "1 changement" : $"{state.DirtyCount} changements";
            if (!status.Success)
                state.Error = status.Error;

            GitResult upstream = InvokeGit(
                repositoryPath,
                new[] { "rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{upstream}" },
                20,
                cancellationToken);
            state.HasUpstream = upstream.Success && !string.IsNullOrWhiteSpace(upstream.Output);

            if (state.HasUpstream)
            {
                GitResult distance = InvokeGit(
                    repositoryPath,
                    new[] { "rev-list", "--left-right", "--count", $"HEAD...{upstream.Output.Trim()}" },
                    30,
                    cancellationToken);
                Match match = Regex.Match(distance.Output, @"^(\d+)\s+(\d+)$");
                if (distance.Success && match.Success)
                {
                    state.Ahead = int.Parse(match.Groups[1].Value);
                    state.Behind = int.Parse(match.Groups[2].Value);
                }
            }

            SetStatus(state);

            GitResult log = InvokeGit(repositoryPath, new[] { "log", "-1", "--format=%an%x1f%aI%x1f%s" }, 20, cancellationToken);
            if (log.Success && !string.IsNullOrWhiteSpace(log.Output))
            {
                string[] parts = log.Output.Split(new[] { (char)0x1f }, 3);
                if (parts.Length >= 1) state.Author = parts[0];
                if (parts.Length >= 2) state.LastCommit = FormatRelativeTime(parts[1]);
                if (parts.Length >= 3) state.Subject = parts[2];
            }

            GitResult remote = InvokeGit(repositoryPath, new[] { "config", "--get", "remote.origin.url" }, 20, cancellationToken);
            if (remote.Success)
                state.Remote = remote.Output.Trim();
            return state;
        }

        static void SetStatus(GitWatchRepositoryState state)
        {
            if (!string.IsNullOrWhiteSpace(state.Error))
            {
                state.StatusKey = GitWatchStatus.Error;
                state.StatusText = "Erreur Git";
            }
            else if (state.Ahead > 0 && state.Behind > 0)
            {
                state.StatusKey = GitWatchStatus.Diverged;
                state.StatusText = $"↕ {state.Ahead} devant · {state.Behind} derrière";
            }
            else if (state.Behind > 0)
            {
                state.StatusKey = GitWatchStatus.Behind;
                state.StatusText = $"↓ {state.Behind} derrière";
            }
            else if (state.Ahead > 0)
            {
                state.StatusKey = GitWatchStatus.Ahead;
                state.StatusText = $"↑ {state.Ahead} devant";
            }
            else if (!state.HasUpstream)
            {
                state.StatusKey = GitWatchStatus.NoUpstream;
                state.StatusText = "Sans suivi distant";
            }
            else
            {
                state.StatusKey = state.DirtyCount > 0 ? GitWatchStatus.Modified : GitWatchStatus.Clean;
                state.StatusText = "À jour";
            }
        }

        static List<string> GetChangedPaths(string repositoryPath, CancellationToken cancellationToken)
        {
            HashSet<string> paths = new HashSet<string>(StringComparer.Ordinal);
            string[][] commands =
            {
                new[] { "diff", "--name-only", "-z", "--" },
                new[] { "diff", "--cached", "--name-only", "-z", "--" },
                new[] { "ls-files", "--others", "--exclude-standard", "-z" }
            };

            foreach (string[] arguments in commands)
            {
                GitResult result = InvokeGit(repositoryPath, arguments, 45, cancellationToken);
                if (!result.Success || string.IsNullOrEmpty(result.Output))
                    continue;
                foreach (string path in result.Output.Split('\0'))
                {
                    if (!string.IsNullOrWhiteSpace(path))
                        paths.Add(path);
                }
            }

            return paths.OrderBy(path => path, StringComparer.Ordinal).ToList();
        }

        static List<GitWatchSuspiciousChange> FindSuspiciousChanges(
            string repositoryPath,
            IEnumerable<string> changedPaths,
            CancellationToken cancellationToken)
        {
            List<GitWatchSuspiciousChange> findings = new List<GitWatchSuspiciousChange>();

            foreach (string relativePath in changedPaths)
            {
                if (!string.Equals(Path.GetExtension(relativePath), ".asset", StringComparison.OrdinalIgnoreCase))
                    continue;

                string absolutePath = Path.Combine(repositoryPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(absolutePath))
                    continue;

                string workingText;
                try { workingText = File.ReadAllText(absolutePath); }
                catch (IOException) { continue; }
                catch (UnauthorizedAccessException) { continue; }

                if (!Regex.IsMatch(workingText, @"(?m)^  m_AtlasPopulationMode: 1\r?$"))
                    continue;

                GitResult head = InvokeGit(repositoryPath, new[] { "show", $"HEAD:{relativePath}" }, 60, cancellationToken);
                if (!head.Success)
                    continue;

                TmpAtlasInfo before = GetTmpAtlasInfo(head.Output);
                TmpAtlasInfo after = GetTmpAtlasInfo(workingText);
                bool wasCleared =
                    before.IsDynamicFontAsset && after.IsDynamicFontAsset && after.ClearDynamicDataOnBuild &&
                    before.Width > 1 && before.Height > 1 && before.GlyphCount > 0 &&
                    after.Width == 1 && after.Height == 1 && after.GlyphTableIsEmpty && after.CharacterTableIsEmpty;

                if (wasCleared)
                {
                    findings.Add(new GitWatchSuspiciousChange
                    {
                        Path = relativePath,
                        Reason = $"Atlas TMP vidé automatiquement ({before.Width}×{before.Height} → 1×1 ; {before.GlyphCount} glyphes supprimés)."
                    });
                }
            }

            return findings;
        }

        static TmpAtlasInfo GetTmpAtlasInfo(string assetText)
        {
            Match texture = Regex.Match(
                assetText,
                @"(?ms)^Texture2D:\s*\r?\n.*?^  m_Width: (?<width>\d+)\r?$.*?^  m_Height: (?<height>\d+)\r?$");
            Match glyphBlock = Regex.Match(assetText, @"(?ms)^  m_GlyphTable:(?<glyphs>.*?)^  m_CharacterTable:");

            return new TmpAtlasInfo
            {
                IsDynamicFontAsset = Regex.IsMatch(assetText, @"(?m)^  m_AtlasPopulationMode: 1\r?$"),
                ClearDynamicDataOnBuild = Regex.IsMatch(assetText, @"(?m)^  m_ClearDynamicDataOnBuild: 1\r?$"),
                Width = texture.Success ? int.Parse(texture.Groups["width"].Value) : 0,
                Height = texture.Success ? int.Parse(texture.Groups["height"].Value) : 0,
                GlyphCount = glyphBlock.Success ? Regex.Matches(glyphBlock.Groups["glyphs"].Value, @"(?m)^  - m_Index: ").Count : 0,
                GlyphTableIsEmpty = Regex.IsMatch(assetText, @"(?m)^  m_GlyphTable: \[\]\r?$"),
                CharacterTableIsEmpty = Regex.IsMatch(assetText, @"(?m)^  m_CharacterTable: \[\]\r?$")
            };
        }

        static void PullSafely(GitWatchRepositoryState state, CancellationToken cancellationToken)
        {
            if (state.DirtyCount > 0)
                state.Operation = "Ignoré : modifications locales";
            else if (!state.HasUpstream)
                state.Operation = "Ignoré : aucune branche distante suivie";
            else if (state.Ahead > 0 && state.Behind > 0)
                state.Operation = "Ignoré : historique divergent";
            else if (state.Behind > 0 && state.Ahead == 0)
            {
                int commitsToPull = state.Behind;
                GitResult pull = InvokeGit(state.Path, new[] { "pull", "--ff-only", "--quiet" }, 180, cancellationToken);
                if (pull.Success)
                {
                    GitWatchRepositoryState refreshed = GetRepositoryState(state.Path, $"Mis à jour · {commitsToPull} commit(s)", cancellationToken);
                    CopyState(refreshed, state);
                }
                else
                {
                    state.Operation = $"Pull impossible : {pull.Error}";
                    state.StatusKey = GitWatchStatus.Error;
                }
            }
            else if (state.Ahead > 0)
                state.Operation = "Aucun pull : dépôt local en avance";
            else
                state.Operation = "Déjà à jour";
        }

        static void RestoreSuspiciousFiles(GitWatchRepositoryState state, CancellationToken cancellationToken)
        {
            string[] paths = state.SuspiciousChanges.Select(change => change.Path).ToArray();
            List<string> arguments = new List<string> { "restore", "--source=HEAD", "--staged", "--worktree", "--" };
            arguments.AddRange(paths);
            GitResult restore = InvokeGit(state.Path, arguments, 120, cancellationToken);

            if (restore.Success)
            {
                GitWatchRepositoryState refreshed = GetRepositoryState(
                    state.Path,
                    paths.Length == 1 ? "1 atlas TMP restauré" : $"{paths.Length} atlas TMP restaurés",
                    cancellationToken);
                CopyState(refreshed, state);
                state.RestoredCount = paths.Length;
            }
            else
            {
                state.Operation = $"Restauration impossible : {restore.Error}";
                state.StatusKey = GitWatchStatus.Error;
            }
        }

        static void CopyState(GitWatchRepositoryState source, GitWatchRepositoryState destination)
        {
            destination.Name = source.Name;
            destination.Path = source.Path;
            destination.Branch = source.Branch;
            destination.DirtyCount = source.DirtyCount;
            destination.LocalText = source.LocalText;
            destination.Ahead = source.Ahead;
            destination.Behind = source.Behind;
            destination.HasUpstream = source.HasUpstream;
            destination.StatusKey = source.StatusKey;
            destination.StatusText = source.StatusText;
            destination.Author = source.Author;
            destination.LastCommit = source.LastCommit;
            destination.Subject = source.Subject;
            destination.Remote = source.Remote;
            destination.Operation = source.Operation;
            destination.Error = source.Error;
            destination.RestoredCount = source.RestoredCount;
            destination.ChangedFiles.Clear();
            destination.ChangedFiles.AddRange(source.ChangedFiles);
            destination.SuspiciousChanges.Clear();
            destination.SuspiciousChanges.AddRange(source.SuspiciousChanges);
        }

        static GitResult InvokeGit(
            string repositoryPath,
            IEnumerable<string> arguments,
            int timeoutSeconds,
            CancellationToken cancellationToken)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "git",
                WorkingDirectory = repositoryPath,
                Arguments = string.Join(" ", arguments.Select(QuoteArgument)),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            startInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";

            using (Process process = new Process { StartInfo = startInfo })
            {
                try
                {
                    process.Start();
                    Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
                    Task<string> stderrTask = process.StandardError.ReadToEndAsync();
                    Stopwatch timeout = Stopwatch.StartNew();

                    while (!process.WaitForExit(100))
                    {
                        if (cancellationToken.IsCancellationRequested || timeout.Elapsed.TotalSeconds >= timeoutSeconds)
                        {
                            try { process.Kill(); } catch { }
                            cancellationToken.ThrowIfCancellationRequested();
                            return GitResult.Failure($"Délai dépassé après {timeoutSeconds} secondes.");
                        }
                    }

                    string stdout = stdoutTask.GetAwaiter().GetResult().TrimEnd('\r', '\n');
                    string stderr = stderrTask.GetAwaiter().GetResult().TrimEnd('\r', '\n');
                    return process.ExitCode == 0
                        ? GitResult.Successful(stdout)
                        : GitResult.Failure(stderr);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception exception)
                {
                    return GitResult.Failure(exception.Message);
                }
            }
        }

        static string QuoteArgument(string argument)
        {
            if (argument.Length == 0)
                return "\"\"";
            if (!argument.Any(character => char.IsWhiteSpace(character) || character == '"' || character == '\\'))
                return argument;
            return "\"" + argument.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        }

        static string DisplayName(string repositoryPath)
        {
            string name = new DirectoryInfo(repositoryPath).Name;
            return name.StartsWith("_", StringComparison.Ordinal) && name.EndsWith("_", StringComparison.Ordinal)
                ? name.Trim('_')
                : name;
        }

        static string FormatRelativeTime(string isoDate)
        {
            if (!DateTimeOffset.TryParse(isoDate, out DateTimeOffset commitDate))
                return isoDate;

            double minutes = Math.Max(0d, (DateTimeOffset.Now - commitDate).TotalMinutes);
            if (minutes < 1d) return "à l’instant";
            if (minutes < 60d) return $"il y a {Math.Floor(minutes)} min";
            if (minutes < 1440d) return $"il y a {Math.Floor(minutes / 60d)} h";
            if (minutes < 10080d) return $"il y a {Math.Floor(minutes / 1440d)} j";
            if (minutes < 43800d) return $"il y a {Math.Floor(minutes / 10080d)} sem.";
            if (minutes < 525600d) return $"il y a {Math.Floor(minutes / 43800d)} mois";
            double years = Math.Floor(minutes / 525600d);
            return years == 1d ? "il y a 1 an" : $"il y a {years} ans";
        }

        sealed class TmpAtlasInfo
        {
            internal bool IsDynamicFontAsset;
            internal bool ClearDynamicDataOnBuild;
            internal int Width;
            internal int Height;
            internal int GlyphCount;
            internal bool GlyphTableIsEmpty;
            internal bool CharacterTableIsEmpty;
        }

        readonly struct GitResult
        {
            internal readonly bool Success;
            internal readonly string Output;
            internal readonly string Error;

            GitResult(bool success, string output, string error)
            {
                Success = success;
                Output = output;
                Error = error;
            }

            internal static GitResult Successful(string output) => new GitResult(true, output, string.Empty);
            internal static GitResult Failure(string error) => new GitResult(false, string.Empty, error ?? string.Empty);
        }
    }
}
#endif
