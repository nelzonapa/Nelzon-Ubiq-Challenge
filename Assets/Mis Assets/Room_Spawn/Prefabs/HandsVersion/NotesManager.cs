using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

public class NotesManager : MonoBehaviour
{
    [Header("Configuración de Guardado")]
    public string notesFolderName = "MeetingNotes";
    public float autoSaveInterval = 30f; // Guardar automáticamente cada 30 segundos

    private Dictionary<string, NoteData> activeNotes = new Dictionary<string, NoteData>();
    private float lastSaveTime;
    private string notesFolderPath;

    [System.Serializable]
    public class NoteData
    {
        public string noteId;
        public string noteText;
        public DateTime creationTime;
        public DateTime lastModifiedTime;
        public Vector3 position;
        public Quaternion rotation;
        public string prefabName;
    }

    void Start()
    {
        // Crear carpeta de notas si no existe
        notesFolderPath = Path.Combine(Application.persistentDataPath, notesFolderName);
        if (!Directory.Exists(notesFolderPath))
        {
            Directory.CreateDirectory(notesFolderPath);
        }

        lastSaveTime = Time.time;
        Debug.Log($"NotesManager iniciado. Carpeta de notas: {notesFolderPath}");
    }

    void Update()
    {
        // Guardado automático periódico
        if (Time.time - lastSaveTime >= autoSaveInterval)
        {
            SaveAllNotes();
            lastSaveTime = Time.time;
        }
    }

    // Registrar una nueva nota
    public void RegisterNote(string noteId, string initialText, Vector3 position, Quaternion rotation, string prefabName)
    {
        if (!activeNotes.ContainsKey(noteId))
        {
            activeNotes[noteId] = new NoteData()
            {
                noteId = noteId,
                noteText = initialText,
                creationTime = DateTime.Now,
                lastModifiedTime = DateTime.Now,
                position = position,
                rotation = rotation,
                prefabName = prefabName
            };

            Debug.Log($"Nota registrada: {noteId}");
        }
    }

    // Actualizar texto de una nota existente
    public void UpdateNote(string noteId, string newText)
    {
        if (activeNotes.ContainsKey(noteId))
        {
            activeNotes[noteId].noteText = newText;
            activeNotes[noteId].lastModifiedTime = DateTime.Now;

            // Guardar inmediatamente al actualizar
            SaveNoteToFile(noteId);
        }
    }

    // Guardar una nota específica en archivo
    private void SaveNoteToFile(string noteId)
    {
        if (activeNotes.ContainsKey(noteId))
        {
            string filePath = Path.Combine(notesFolderPath, $"{noteId}.json");
            string jsonData = JsonUtility.ToJson(activeNotes[noteId], true);

            try
            {
                File.WriteAllText(filePath, jsonData);
                Debug.Log($"Nota guardada: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error al guardar nota {noteId}: {e.Message}");
            }
        }
    }

    // Guardar todas las notas
    public void SaveAllNotes()
    {
        foreach (string noteId in activeNotes.Keys)
        {
            SaveNoteToFile(noteId);
        }
        Debug.Log($"Todas las notas guardadas ({activeNotes.Count} notas)");
    }

    // Cargar todas las notas guardadas (útil al iniciar la aplicación)
    public void LoadAllNotes()
    {
        if (Directory.Exists(notesFolderPath))
        {
            string[] noteFiles = Directory.GetFiles(notesFolderPath, "*.json");
            foreach (string filePath in noteFiles)
            {
                try
                {
                    string jsonData = File.ReadAllText(filePath);
                    NoteData note = JsonUtility.FromJson<NoteData>(jsonData);
                    activeNotes[note.noteId] = note;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error al cargar nota {filePath}: {e.Message}");
                }
            }
            Debug.Log($"Notas cargadas: {noteFiles.Length}");
        }
    }

    // Obtener todas las notas (para UI o exportación)
    public List<NoteData> GetAllNotes()
    {
        return new List<NoteData>(activeNotes.Values);
    }

    // Eliminar una nota
    public void DeleteNote(string noteId)
    {
        if (activeNotes.ContainsKey(noteId))
        {
            activeNotes.Remove(noteId);
            string filePath = Path.Combine(notesFolderPath, $"{noteId}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    // Al cerrar la aplicación, guardar todo
    void OnApplicationQuit()
    {
        SaveAllNotes();
    }

    void OnDestroy()
    {
        SaveAllNotes();
    }
}