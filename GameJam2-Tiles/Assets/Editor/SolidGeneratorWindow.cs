using System.Text;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System;
using Unity.Entities.UniversalDelegates;
using UnityEngine.UI;

namespace XGD.TileQuest
{
    public struct GUIStylePack
    {
        public Color colDefault;
        public Color colInactive;
        public Color colHighlight;
        public Color colHeader;
        public EditorWindow window;
    }

    public class FileUIElement
    {
        public FileInfo file;
        public string label;
        public bool selected;
        public string extension;
        public string group = "";
        public List<string> tagList;

        public FileUIElement(FileInfo file, string extension)
        {
            this.file = file;
            this.extension = extension;
            label = file.Name.Replace(extension, "");
        }

        public static List<List<FileUIElement>> Group(List<FileUIElement> list)
        {
            List<string> groupNames = new List<string>();
            List<List<FileUIElement>> groupedLists = new List<List<FileUIElement>>();

            foreach (FileUIElement x in list)
            {
                string groupName = x.group;
                int id = groupNames.IndexOf(groupName);

                if (id == -1)
                {
                    groupNames.Add(groupName);
                    groupedLists.Add(new List<FileUIElement>());
                    id = groupNames.Count - 1;
                }
                groupedLists[id].Add(x);
            }
            return groupedLists.Where(x => x.Count > 0).ToList();
        }
    }

    public class SelectionPanel
    {
        public Vector2 scrollPos;
        public DirectoryInfo directory;
        public List<FileUIElement> view;
        public List<FileUIElement> folder;
        public List<FileUIElement> search;
        public List<FileUIElement> selected;
        public FileUIElement active;
        public string name;
        public string rootPath;
        public string extension;
        public string searchbar;
        public string changed_searchbar;
        public bool refreshFolder;
        public bool refreshSearch;
        public bool refreshView;
        public float width;

        public bool groupPolyhedrons;

        public bool selectMode_range;
        public bool selectMode_add;

        public List<string> polyhedronClassNames;
        public List<List<string>> polyhedronClassMembers;

        public SelectionPanel(string name, string rootPath, string extension, float width, string searchEntry)
        {
            this.name = name;
            this.rootPath = rootPath;
            this.extension = extension;
            this.width = width;
            this.searchbar = searchEntry;

            searchbar = "";

            view = new List<FileUIElement>();
            folder = new List<FileUIElement>();
            search = new List<FileUIElement>();
            selected = new List<FileUIElement>();

            scrollPos = Vector3.zero;
        }

        public string[] files
        {
            get
            {
                return selected.Select(x => x.file.FullName).ToArray();
            }
        }

        private string[] ReadSolidClassFile(FileInfo file)
        {
            string line;
            List<string> names = new List<string>();
            StreamReader reader = new StreamReader(file.FullName, Encoding.Default);
            SolidFileBuffer buffer = new SolidFileBuffer();

            using (reader)
            {
                while ((line = reader.ReadLine()) != null)
                {
                    names.Add(line);
                }
            }

            return names.ToArray();
        }



        public void DrawPanel(GUIStylePack stylePack)
        {

            Event e = Event.current;
            selectMode_range = e.shift;
            selectMode_add = e.control;

            GUILayout.BeginVertical(GUILayout.MinWidth(width));
            GUILayout.Label(name, EditorStyles.boldLabel);
            GUILayout.Label("");

            groupPolyhedrons = EditorGUILayout.Toggle("Group Polyhedrons", groupPolyhedrons);

            //Search
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            changed_searchbar = EditorGUILayout.TextField(searchbar, GUILayout.MaxWidth(410));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool all = GUILayout.Button("Select All", GUILayout.MaxWidth(200));
            bool none = GUILayout.Button("Deselect All", GUILayout.MaxWidth(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Label("");

            if (directory == null) refreshFolder = true;
            if (!changed_searchbar.Equals(searchbar)) refreshSearch = true;

            if (refreshFolder || refreshSearch)
            {
                searchbar = changed_searchbar;
                //((SolidGeneratorWindow)stylePack.window).searchbar = searchbar;
                RefreshSearch();
            }

            if (all || none)
            {
                view.ForEach(x => x.selected = all);
                if (all)
                {
                    // Add all visible unselected elements
                    selected.AddRange(view.Where(x => !selected.Contains(x)));
                }
                else if (none)
                {
                    // Remove all visible selected elements
                    view.Where(x => selected.Contains(x)).ToList().ForEach(x => selected.Remove(x));
                }
            }

            // Selection List
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.MaxHeight(500));
            if (view.Count > 0)
            {
                if (groupPolyhedrons)
                {
                    FileUIElement.Group(view).ForEach(x => DrawFileGroup(x, stylePack));
                }
                else
                {
                    DrawFileGroup(view, stylePack);
                }
            }
            else
            {
                GUILayout.Label("");
                GUILayout.Label("");
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("No matching solids found.");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Label("");
                GUILayout.Label("");
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        void SelectFile(FileUIElement button)
        {
            bool single = (!selectMode_range && !selectMode_add) || (!selectMode_add && selectMode_range && active == null);
            bool addRange = selectMode_add && selectMode_range;
            

            if (single)
            {
                Debug.Log("single");
                selected.ForEach(x => x.selected = false);
                selected.Clear();
                if (!button.selected)
                {
                    button.selected = true;
                    selected.Add(button);
                    active = button;
                }
                else
                {
                    button.selected = false;
                    if (button == active) active = null;
                }
            }
            else if (selectMode_add && !selectMode_range)
            {
                Debug.Log("a_s");
                if (button.selected)
                {
                    button.selected = false;
                    selected.Remove(button);
                }
                else
                {
                    button.selected = true;
                    selected.Add(button);
                }
            }
            else if (addRange && active != null)
            {
                Debug.Log("a_m");
                int start = view.IndexOf(active);
                int end = view.IndexOf(button);

                List<FileUIElement> buttons = new List<FileUIElement>();
                for (int i = start; i <= end; i++) buttons.Add(view[i]);
                buttons = buttons.Where(x => !selected.Contains(x)).ToList();

                foreach (FileUIElement x in buttons)
                {
                    x.selected = true;
                    selected.Add(x);
                }
            }
            else if (selectMode_range && active !=null)
            {
                Debug.Log("r_m");
                selected.ForEach(x => x.selected = false);
                selected.Clear();                

                int start = view.IndexOf(active);
                int end = view.IndexOf(button);
                int len = end - start;

                List<FileUIElement> buttons = new List<FileUIElement>();
                for (int i = 0; i <= Mathf.Abs(len); i++) buttons.Add(view[start + (int)Mathf.Sign(len) * i]);

                foreach (FileUIElement x in buttons)
                {
                    x.selected = true;
                    selected.Add(x);
                }
            }

        }

        void DrawFileGroup(List<FileUIElement> list, GUIStylePack stylePack)
        {
            string groupName = list[0].group == "" ? "Group" : list[0].group;
            GUI.color = stylePack.colHeader;
            GUILayout.Label(string.Format("{0}({1})", groupName, list.Count));
            GUI.color = stylePack.colDefault;

            for (int i = 0; i < list.Count; i++)
            {
                FileUIElement button = view[i];
                GUI.color = button.selected ? stylePack.colHighlight : stylePack.colDefault;

                if (GUILayout.Button(button.label))
                {
                    refreshView = true;
                    SelectFile(button);
                }
                GUI.color = stylePack.colDefault;
            }
        }

        private void UpdatePolyheadronClasses()
        {
            DirectoryInfo cDir = new DirectoryInfo(string.Format(@"{0}{1}", rootPath, "Solid Classification"));
            FileInfo[] classFiles = cDir.GetFiles("*.txt", SearchOption.AllDirectories);
            List<string> names = new List<string>();
            List<List<string>> allEntries = new List<List<string>>();

            for (int i = 0; i < classFiles.Length; i++)
            {
                names.Add(classFiles[i].Name.Replace(".txt", ""));
                allEntries.Add(new List<string>(ReadSolidClassFile(classFiles[i])));
            }
            polyhedronClassNames = names.ToList();
            polyhedronClassMembers = allEntries.ToList();
        }

        private List<string> GetPolyhedronMembers(string className)
        {
            for (int i = 0; i < polyhedronClassNames.Count; i++)
            {
                if (className.Equals(polyhedronClassNames[i]))
                    return polyhedronClassMembers[i];
            }
            return new List<string>();
        }

        public void RefreshFolder()
        {
            refreshFolder = false;

            directory = new DirectoryInfo(string.Format(@"{0}/{1}", rootPath, name));

            if (directory.Exists)
            {
                folder.Clear();
                FileInfo[] fileInfos = directory.GetFiles("*" + extension, SearchOption.TopDirectoryOnly);
                folder = fileInfos.Select(x => new FileUIElement(x, extension)).ToList();
                
                foreach(FileUIElement file in folder)
                {
                    for(int i = 0; i < polyhedronClassNames.Count; i++)
                    {
                        if (polyhedronClassMembers[i].Contains(file.label))
                        {
                            file.group = polyhedronClassNames[i];
                        }
                    }
                }
            }
        }

        private void RefreshSearch()
        {
            UpdatePolyheadronClasses();

            if (searchbar == "")
            {
                RefreshFolder();
                search = folder;
            }
            else
            {
                // Get user search tags
                List<string> userSearchTags = searchbar.Split(' ').ToList();

                // Get classification files that match user search tags

                // Get names in classes
                List<string> searchQueries = new List<string>();
                userSearchTags.ForEach(tag => searchQueries.AddRange(GetPolyhedronMembers(tag)));
                searchQueries.AddRange(userSearchTags);

                // Get results from all queries
                List<FileUIElement> searchResults = new List<FileUIElement>();
                searchQueries.ForEach(query => searchResults.AddRange(folder
                    .Where(entry => entry.label.ToLower().Equals(query.ToLower())).ToList()));
                search = searchResults;
            }
            refreshSearch = false;
            RefreshView();
        }

        private void RefreshView()
        {
            refreshView = false;

            List<FileUIElement> changed_view = new List<FileUIElement>();
            changed_view.AddRange(selected.Where(x => !search.Contains(x)));
            changed_view.AddRange(search);

            view = changed_view;
        }
    }

    public class SolidGeneratorWindow : EditorWindow
    {
        GUIStylePack stylePack;

        public ImageRenderQuery query;
        public int textureWidth;
        public int textureHeight;
        public TextureFormat textureFormat;
        public GameObject tileBase;
        public GameObject solidBase;
        public GameObject cameraPrefab;
        public Transform cameraAnchor;
        public Vector2 scrollImageGrid;


        public List<SelectionPanel> selectionPanels;
        public static SolidGeneratorWindow window;
        public string solidCollectionName;
        public static float maxWidth = 450.0f;
        public string searchbar;


        void OnGUI()
        {
            bool generate = false;

            // Render Preview
            GUILayout.Label("Solid Collection", EditorStyles.boldLabel);
            DrawTextureGrid(query.previewTextures, 3);
            GUILayout.Label("", GUILayout.Height(5));

            // Generation Options
            GUILayout.Label("Generation", EditorStyles.boldLabel);
            solidCollectionName = EditorGUILayout.TextField("Name", solidCollectionName);
            AssignPrefab("Tile Prefab", ref tileBase);
            AssignPrefab("Solid Prefab", ref solidBase);
            GUILayout.Label("");
            GUILayout.Label("Rendering", EditorStyles.boldLabel);
            AssignTextureFormat("Texture Format", ref textureFormat);
            AssignPrefab("Camera Prefab", ref cameraPrefab);
            AssignTransform("Camera Anchor", ref cameraAnchor);


            if (selectionPanels == null || selectionPanels.Contains(null) || !window) Init();

            if (!selectionPanels.Contains(null))
            {
                SelectionPanel resources = selectionPanels[0];
                SelectionPanel assets = selectionPanels[1];

                GUILayout.Label("");
                generate &= resources.selected.Count > 0 && tileBase && tileBase;

                // SelectionPanels 
                GUILayout.BeginHorizontal();
                //selectionPanels.ForEach(x => x.DrawPanel(stylePack));
                resources.DrawPanel(stylePack);
                GUILayout.EndHorizontal();
                GUILayout.Label("");

                // Generate Button
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                generate |= GUILayout.Button("Generate", GUILayout.Height(30), GUILayout.MaxWidth(300));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();


                if (generate)
                {
                    SolidCollection solidCollection;
                    string assetPath;

                    // Create asset
                    solidCollection = CreateInstance<SolidCollection>();
                    solidCollection.basePrefabTiles = tileBase;
                    solidCollection.basePrefabSolids = solidBase;
                    solidCollection.name = solidCollectionName;
                    assetPath = string.Format(@"{0}\{1}.{2}", "Assets/Resources/Solid Collections", solidCollectionName, "asset");
                    AssetDatabase.CreateAsset(solidCollection, assetPath);
                    solidCollection.CreateSolids(resources.files);

                    // Generation
                    GameObject collection = solidCollection.Generate();
                    Vector3 cPos = Vector3.zero;
                    Vector3 cRot = Vector3.zero;

                    if (cameraAnchor)
                    {
                        cPos = cameraAnchor.position;
                        cRot = cameraAnchor.eulerAngles;
                    }

                    //Rendering
                    query = new ImageRenderQuery()
                    {
                        textureFormat = textureFormat,
                        textureWidth = 1024,
                        textureHeight = 1024,
                        cameraPosition = cPos,
                        cameraRotation = cRot,
                        targets = collection.transform.GetComponentsInChildren<SolidBehavior>().Select(x => x.gameObject).ToList(),
                        camera = cameraPrefab,
                        previewTextures = new List<Texture2D>()
                    };

                    query.Render();
                    assets.refreshFolder = true;
                }
            }



        }
        [MenuItem("Window/Experimental Game Design/SolidGenerator")]
        static void Init()
        {
            if (!window)
            {
                window = (SolidGeneratorWindow)GetWindow(typeof(SolidGeneratorWindow));
            }
            window.stylePack = new GUIStylePack
            {
                colDefault = GUI.color,
                colInactive = Color.Lerp(GUI.color, Color.black, 0.5f),
                colHighlight = Color.Lerp(GUI.color, Color.green, 0.5f),
                colHeader = Color.white,
                window = window

            };

            window.selectionPanels = new List<SelectionPanel>()
        {
            new SelectionPanel("Solid Resources", "Assets/Resources/", ".txt", maxWidth, window.searchbar),
            new SelectionPanel("Solid Collections", "Assets/Resources/", ".asset", 200, "")
        };

            window.Show();

        }

        void DrawTextureGrid(List<Texture2D> textures, int n)
        {
            if (textures == null || textures.Count == 0)
            {
                Texture2D defaultTexture = new Texture2D((int)maxWidth, (int)maxWidth);

                float ratio = Mathf.Clamp01(defaultTexture.width / maxWidth);
                float size = ratio * maxWidth;

                GUILayout.Label(defaultTexture, GUILayout.Width(size), GUILayout.Height(size));
            }
            else
            {
                float ratio = Mathf.Clamp01((textures[0].width / n) / (maxWidth));
                float size = ratio * maxWidth / n;

                scrollImageGrid = GUILayout.BeginScrollView(scrollImageGrid, GUILayout.MaxHeight(400));

                GUILayout.BeginVertical();
                for (int v = 0; v < textures.Count / n; v++)
                {
                    GUILayout.BeginHorizontal();
                    int m = Mathf.Clamp(textures.Count - n * v, 0, 3);
                    for (int u = 0; u < m; u++)
                    {
                        GUILayout.BeginVertical();
                        Texture2D texture = textures[u + v];
                        GUILayout.Label(texture, GUILayout.Width(size), GUILayout.Height(size));
                        //GUILayout.Label(texture.name.Split('_')[1]);
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }

        }

        void AssignPrefab(string name, ref GameObject obj)
        {
            obj = (GameObject)EditorGUILayout.ObjectField(name, (UnityEngine.Object)obj, typeof(GameObject), false);
        }

        void AssignMaterial(string name, ref Material mat)
        {
            mat = (Material)EditorGUILayout.ObjectField(name, (UnityEngine.Object)mat, typeof(Material), false);
        }

        void AssignTransform(string name, ref Transform t)
        {
            t = (Transform)EditorGUILayout.ObjectField(name, (UnityEngine.Object)t, typeof(Transform), true);
        }

        void AssignTextureFormat(string name, ref TextureFormat tf)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(name);
            tf = (TextureFormat)EditorGUILayout.EnumPopup(tf);
            GUILayout.EndHorizontal();
        }

    }
}