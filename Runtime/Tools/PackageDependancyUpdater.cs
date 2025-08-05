using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace _UTIL_e
{
    internal static class PackageJsonUpdater
    {
        const string button_name = "Assets/" + nameof(_EDITOR_) + "/" + nameof(UpdatePackageJson);

        //--------------------------------------------------------------------------------------------------------------

        [MenuItem(button_name)]
        static void UpdatePackageJson()
        {
            // On récupère le fichier sélectionné dans l'onglet Project
            string asmdefPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(asmdefPath) || !asmdefPath.EndsWith(".asmdef"))
            {
                Debug.LogError("Sélectionne un fichier .asmdef");
                return;
            }

            // Lecture du fichier asmdef et parsing avec JsonUtility
            string asmdefText = File.ReadAllText(asmdefPath);
            AsmDef asmdefData = JsonUtility.FromJson<AsmDef>(asmdefText);
            if (asmdefData == null || asmdefData.references == null || asmdefData.references.Length == 0)
            {
                Debug.LogWarning("Aucune référence trouvée dans l'asmdef.");
                return;
            }

            // Recherche récursive du package.json à partir du dossier de l'asmdef et en remontant les dossiers parents
            string directory = Path.GetDirectoryName(asmdefPath);
            string packageJsonPath = null;
            while (!string.IsNullOrEmpty(directory))
            {
                string possiblePath = Path.Combine(directory, "package.json");
                if (File.Exists(possiblePath))
                {
                    packageJsonPath = possiblePath;
                    break;
                }
                directory = Directory.GetParent(directory)?.FullName;
            }
            if (string.IsNullOrEmpty(packageJsonPath))
            {
                Debug.LogError("Aucun package.json trouvé dans le dossier ou les dossiers parents !");
                return;
            }

            // Pour chaque référence de l'asmdef, on cherche une version dans les fichiers du projet
            Dictionary<string, string> newDependencies = new Dictionary<string, string>();
            foreach (string reference in asmdefData.references)
            {
                string version = FindVersionForDependency(reference);
                if (!string.IsNullOrEmpty(version))
                {
                    newDependencies[reference] = version;
                    Debug.Log($"Dépendance ajoutée : {reference} => {version}");
                }
                else
                {
                    Debug.LogWarning($"Version non trouvée pour : {reference}");
                }
            }

            // Lecture du package.json en mode texte
            string packageJsonText = File.ReadAllText(packageJsonPath);

            // On construit une chaîne JSON pour le bloc "dependencies"
            string newDependenciesJson = BuildDependenciesJson(newDependencies);

            // Expression régulière pour trouver le bloc "dependencies" existant (sur plusieurs lignes)
            Regex dependenciesRegex = new Regex("\"dependencies\"\\s*:\\s*\\{.*?\\}", RegexOptions.Singleline);
            if (dependenciesRegex.IsMatch(packageJsonText))
            {
                // Remplacement du bloc existant
                packageJsonText = dependenciesRegex.Replace(packageJsonText, "\"dependencies\": " + newDependenciesJson);
            }
            else
            {
                // S'il n'y a pas de bloc dependencies, on l'ajoute juste avant le dernier '}'
                int lastBraceIndex = packageJsonText.LastIndexOf('}');
                if (lastBraceIndex >= 0)
                {
                    packageJsonText = packageJsonText.Insert(lastBraceIndex, ",\n  \"dependencies\": " + newDependenciesJson + "\n");
                }
                else
                {
                    Debug.LogError("Format inattendu du package.json !");
                    return;
                }
            }

            // Écriture du package.json mis à jour et rafraîchissement de l'AssetDatabase
            File.WriteAllText(packageJsonPath, packageJsonText, Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("Mise à jour de package.json terminée avec JsonUtility ! 🎉");
        }

        /// <summary>
        /// Parcourt les fichiers du projet (dans Assets) pour trouver une occurence du pattern "dependencyName : vX.Y.Z".
        /// </summary>
        /// <param name="dependencyName">Nom de la dépendance recherchée</param>
        /// <returns>La version trouvée (ex: "v2.0.36") ou null si rien n'est trouvé</returns>
        private static string FindVersionForDependency(string dependencyName)
        {
            string assetsPath = Application.dataPath;
            string[] files = Directory.GetFiles(assetsPath, "*.*", System.IO.SearchOption.AllDirectories);

            // On cherche le pattern (insensible à la casse)
            Regex regex = new Regex(Regex.Escape(dependencyName) + @"\s*:\s*v?(\d+\.\d+\.\d+)", RegexOptions.IgnoreCase);
            foreach (string file in files)
            {
                // On limite la recherche aux fichiers de code ou texte pour éviter de lire des binaires
                if (!(file.EndsWith(".cs") || file.EndsWith(".asmdef") || file.EndsWith(".json") || file.EndsWith(".txt")))
                    continue;

                string content = File.ReadAllText(file);
                Match match = regex.Match(content);
                if (match.Success)
                {
                    // On renvoie le numéro de version avec le préfixe "v"
                    return "v" + match.Groups[1].Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Construit une chaîne JSON pour représenter le bloc "dependencies" à partir d'un dictionnaire.
        /// </summary>
        private static string BuildDependenciesJson(Dictionary<string, string> dependencies)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");
            int count = 0;
            foreach (KeyValuePair<string, string> pair in dependencies)
            {
                sb.AppendFormat("    \"{0}\": \"{1}\"", pair.Key, pair.Value);
                count++;
                if (count < dependencies.Count)
                    sb.Append(",\n");
                else
                    sb.Append("\n");
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}