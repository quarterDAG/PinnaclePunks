using UnityEngine;


public class PlayerParticlesAndAudio : MonoBehaviour
{
    [Header("Particles")][SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _landParticles;
    [SerializeField] private ParticleSystem _hitParticles;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip[] _footsteps;

    private AudioSource _source;
    private IPlayerController _player;
    private bool _grounded;
    private ParticleSystem.MinMaxGradient _currentGradient;

    private void Awake ()
    {
        _source = GetComponent<AudioSource>();
        _player = GetComponentInParent<IPlayerController>();
    }

    private void OnEnable ()
    {
        _player.Jumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;
        _player.Hit += OnHit;

        _moveParticles.Play();
    }

    private void OnDisable ()
    {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.Hit -= OnHit;

        _moveParticles.Stop();
    }


    private void OnJumped ()
    {
        if (_grounded) // Avoid coyote
        {
            _jumpParticles.Play();
        }
    }

    private void OnHit ()
    {

        _hitParticles.Play();

    }

    private void OnGroundedChanged ( bool grounded, float impact )
    {
        _grounded = grounded;

        if (grounded)
        {
            _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
            _moveParticles.Play();

            _landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            _landParticles.Play();

            Debug.Log("Land Particles");
        }
        else
        {
            _moveParticles.Stop();
        }
    }


    public void SetColor ( Color color )
    {
        var main = _jumpParticles.main;
        main.startColor = color;

        main = _launchParticles.main;
        main.startColor = color;

        main = _moveParticles.main;
        main.startColor = color;

        main = _landParticles.main;
        main.startColor = color;

        main = _hitParticles.main;
        main.startColor = color;


    }
}
