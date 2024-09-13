using UnityEngine;
using UnityEngine.UI;  // Adicione esta linha para corrigir o erro de Button e Slider
using TMPro;          // Para o TextMeshPro


public class JukeboxManager : MonoBehaviour
{
    public AudioSource audioSource;         // Controla a m�sica que ser� tocada
    public AudioClip[] musicClips;          // Array de m�sicas dispon�veis
    public Slider volumeSlider;             // Slider para controlar o volume
    public Button playPauseButton;          // Bot�o Play/Pause
    public TMP_Text trackNameText;          // Texto onde o nome da m�sica ser� exibido (TMP)
    public float scrollSpeed = 50f;         // Velocidade do texto de deslizar

    private int currentTrackIndex = 0;      // �ndice da m�sica atual
    private bool isPaused = false;          // Indica se a m�sica est� pausada
    private RectTransform trackTextRect;    // Refer�ncia ao RectTransform do texto

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

        // Adiciona listener ao bot�o Play/Pause
        playPauseButton.onClick.AddListener(TogglePlayPause);

        // Refer�ncia ao RectTransform do texto (TextMeshPro)
        trackTextRect = trackNameText.GetComponent<RectTransform>();

        // Desativa o loop de cada m�sica para poder mudar automaticamente
        audioSource.loop = false;
    }

    void Update()
    {
        // Verifica se a m�sica terminou
        if (!audioSource.isPlaying && !isPaused)
        {
            NextTrack();
        }

        // Aplicar o efeito de deslizamento no nome da m�sica
        ScrollTrackName();
    }

    // Fun��o para tocar a m�sica
    public void PlayMusic(int index)
    {
        audioSource.clip = musicClips[index];
        audioSource.Play();
        isPaused = false;

        // Atualiza o nome da m�sica no UI (TextMeshPro)
        trackNameText.text = musicClips[index].name;

        // Resetar a posi��o do texto para o in�cio
        trackTextRect.anchoredPosition = new Vector2(0, trackTextRect.anchoredPosition.y);
    }

    // Fun��o para alternar entre Play e Pause
    public void TogglePlayPause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            isPaused = true;
        }
        else
        {
            audioSource.Play();
            isPaused = false;
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

    // Fun��o para deslizar o texto do nome da m�sica
    private void ScrollTrackName()
    {
        if (trackNameText.preferredWidth > trackTextRect.rect.width)
        {
            // Desliza o texto para a esquerda
            trackTextRect.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

            // Se o texto sair completamente da tela, volta ao in�cio
            if (trackTextRect.anchoredPosition.x + trackNameText.preferredWidth < 0)
            {
                trackTextRect.anchoredPosition = new Vector2(trackTextRect.rect.width, trackTextRect.anchoredPosition.y);
            }
        }
    }
}
