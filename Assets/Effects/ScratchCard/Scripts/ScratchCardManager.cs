using UnityEngine;
using UnityEngine.UI;

public class ScratchCardManager : MonoBehaviour
{
	public enum ScratchCardRenderType
	{
		MeshRenderer,
		SpriteRenderer,
		CanvasRenderer
	}
	
	public Camera MainCamera;
	public ScratchCardRenderType RenderType;
	public Sprite ScratchSurfaceSprite;
	public Texture EraseTexture;
	
	public ScratchCard Card;
	public EraseProgress Progress;
	public GameObject MeshCard;
	public GameObject SpriteCard;
	public GameObject ImageCard;
	public Shader MaskShader;
	public Shader BrushShader;
	public Shader MaskProgressShader;
	
	private Material scratchSurfaceMaterial;
	private Material eraserMaterial;
	private Material progressMaterial;
	
	void Awake()
	{
		if (Card.MainCamera == null)
		{
			Card.MainCamera = MainCamera;
		}
		
		if (Card.ScratchSurface == null)
		{
			scratchSurfaceMaterial = new Material(MaskShader);
			scratchSurfaceMaterial.mainTexture = ScratchSurfaceSprite.texture;
			Card.ScratchSurface = scratchSurfaceMaterial;
		}
		
		if (Card.Eraser == null)
		{
			eraserMaterial = new Material(BrushShader);
			eraserMaterial.mainTexture = EraseTexture;
			Card.Eraser = eraserMaterial;
		}
		
		if (Card.Progress == null)
		{
			progressMaterial = new Material(MaskProgressShader);
			Card.Progress = progressMaterial;
		}
		
		if (RenderType == ScratchCardRenderType.MeshRenderer)
		{
			MeshCard.SetActive(true);
			SpriteCard.SetActive(false);
			ImageCard.SetActive(false);
			Card.Surface = MeshCard.transform;
			MeshCard.GetComponent<Renderer>().material = scratchSurfaceMaterial;
		}
		else if (RenderType == ScratchCardRenderType.SpriteRenderer)
		{
			MeshCard.SetActive(false);
			SpriteCard.SetActive(true);
			ImageCard.SetActive(false);
			Card.Surface = SpriteCard.transform;
			var sprite = SpriteCard.GetComponent<SpriteRenderer>();
			sprite.sprite = ScratchSurfaceSprite;
			sprite.material = scratchSurfaceMaterial;
		}
		else
		{
			MeshCard.SetActive(false);
			SpriteCard.SetActive(false);
			ImageCard.SetActive(true);
			Card.Surface = ImageCard.transform;
			var image = ImageCard.GetComponent<Image>();
			image.sprite = ScratchSurfaceSprite;
			image.material = scratchSurfaceMaterial;
		}
	}
	
	public void SetEraseTexture(Texture texture)
	{
		eraserMaterial.mainTexture = texture;
	}

    public void ResetScratchCard()
    {
        Card.Reset();
    }
}