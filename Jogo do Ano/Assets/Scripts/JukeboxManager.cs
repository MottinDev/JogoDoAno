using UnityEngine;
using UnityEngine.UI;

public class JukeboxManager : MonoBehaviour
{
    public AudioSource audioSource; // Controla a m�sica que ser� tocada
    public AudioClip[] musicClips;  // Array de m�sicas dispon�veis
    public Slider volumeSlider;     // Slider para controlar o volume

    private int currentTrackIndex = 0; // �ndice da m�sica atual

    void Start()
    {
        if (musicClips.Length > 0)
        {
            PlayMusic(currentTrackIndex);
        }

        // Configura o volume inicial com base no slider
        audioSource.volume = volumeSlider.value;

        // Detecta mudan�as no slider de volume
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    // Fun��o para tocar a m�sica
    public void PlayMusic(int index)
    {
        audioSource.clip = musicClips[index];
        audioSource.Play();
    }

    // Pausar ou continuar m�sica
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

    // Trocar para a pr�xima m�sica
    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicClips.Length; // Avan�a para o pr�ximo, com loop
        PlayMusic(currentTrackIndex);
    }

    // Trocar para a m�sica anterior
    public void PreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = musicClips.Length - 1; // Volta para a �ltima m�sica se for menor que 0
        }
        PlayMusic(currentTrackIndex);
    }

    // Regular o volume
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
