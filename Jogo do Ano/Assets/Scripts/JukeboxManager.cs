using UnityEngine;
using UnityEngine.UI;  // Adiciona suporte ao Button e Slider
using TMPro;          // Para o TextMeshPro

public class JukeboxManager : MonoBehaviour
{
    public AudioSource audioSource;         // Controla a música que será tocada
    public AudioClip[] musicClips;          // Array de músicas disponíveis
    public Slider volumeSlider;             // Slider para controlar o volume
    public Button playPauseButton;          // Botão Play/Pause
    public TMP_Text trackNameText;          // Texto onde o nome da música será exibido (TMP)

    private int currentTrackIndex = 0;      // Índice da música atual
    private bool isPaused = false;          // Indica se a música está pausada

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
