using UnityEngine;
using UnityEngine.UI;

public class JukeboxManager : MonoBehaviour
{
    public AudioSource audioSource; // Controla a música que será tocada
    public AudioClip[] musicClips;  // Array de músicas disponíveis
    public Slider volumeSlider;     // Slider para controlar o volume

    private int currentTrackIndex = 0; // Índice da música atual

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
    }

    // Função para tocar a música
    public void PlayMusic(int index)
    {
        audioSource.clip = musicClips[index];
        audioSource.Play();
    }

    // Pausar ou continuar música
    public void TogglePause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
        else
        {
            audioSource.Play();
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
