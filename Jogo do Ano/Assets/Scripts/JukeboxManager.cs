using UnityEngine;
using UnityEngine.UI;  
using TMPro;          

public class JukeboxManager : MonoBehaviour
{
    public AudioSource audioSource;         
    public AudioClip[] musicClips;          
    public Slider volumeSlider;            
    public Button playPauseButton;         
    public TMP_Text trackNameText;         

    private int currentTrackIndex = 0;      
    private bool isPaused = false;          

    void Start()
    {
        if (musicClips.Length > 0)
        {
            PlayMusic(currentTrackIndex);
        }

        // Configura o volume inicial com base no slider
        audioSource.volume = volumeSlider.value;

        // Detecta mudanças no slider de volume
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Adiciona listener ao botão Play/Pause
        playPauseButton.onClick.AddListener(TogglePlayPause);

        // Desativa o loop de cada música para poder mudar automaticamente
        audioSource.loop = false;
    }

    void Update()
    {
        // Verifica se a música terminou
        if (!audioSource.isPlaying && !isPaused)
        {
            NextTrack();
        }
    }

    // Função para tocar a música
    public void PlayMusic(int index)
    {
        audioSource.clip = musicClips[index];
        audioSource.Play();
        isPaused = false;

        // Atualiza o nome da música no UI (TextMeshPro)
        trackNameText.text = musicClips[index].name;
    }

    // Função para alternar entre Play e Pause
    public void TogglePlayPause()
    {
        // Verifica se o áudio está pausado ou tocando
        if (isPaused)
        {
            // Retoma a reprodução
            audioSource.Play();
            isPaused = false;
        }
        else if (audioSource.isPlaying)
        {
            // Pausa a reprodução
            audioSource.Pause();
            isPaused = true;
        }
        else
        {
            // Caso o áudio tenha parado, toca novamente
            audioSource.Play();
            isPaused = false;
        }
    }

    // Trocar para a próxima música
    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicClips.Length; // Avança para o próximo, com loop
        PlayMusic(currentTrackIndex);
    }

    // Trocar para a música anterior
    public void PreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = musicClips.Length - 1; // Volta para a última música se for menor que 0
        }
        PlayMusic(currentTrackIndex);
    }

    // Regular o volume
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
