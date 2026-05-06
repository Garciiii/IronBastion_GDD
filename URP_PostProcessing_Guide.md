# URP Post-Processing Guide for a "Mystic" Environment

This guide will walk you through setting up post-processing effects in the Universal Render Pipeline (URP) to achieve a stylized, "mystic" aesthetic for your Tower Defense game. We'll focus on Bloom, Tonemapping (ACES), and Vignette.

### 1. Prerequisites

*   **URP Installed:** Make sure your project is using the Universal Render Pipeline. You can verify this in `Project Settings > Graphics`.
*   **Post-processing Enabled:** In your URP Asset (`Project Settings > Graphics > Scriptable Render Pipeline Settings`), ensure that `Post-processing` is enabled.
*   **Volume Component on Camera:** Your main camera needs a `Volume` component to apply the post-processing effects.

### 2. Setting up the Global Volume

1.  **Create a Global Volume:** In your scene, create a new empty GameObject named `Global Volume`.
2.  **Add a Volume Component:** With the `Global Volume` GameObject selected, add a `Volume` component in the Inspector.
3.  **Create a Volume Profile:**
    *   In the `Volume` component, click the `New` button next to the `Profile` field.
    *   This will create a new Volume Profile asset in your project. Name it something like `Mystic_PostProcessing_Profile`.
4.  **Set the Volume to Global:** In the `Volume` component, check the `Is Global` box. This will apply the effects to the entire scene.

### 3. Configuring the Effects

Now, let's add and configure the post-processing overrides to the `Mystic_PostProcessing_Profile`.

1.  **Open the Profile:** Select the `Global Volume` GameObject and in the `Volume` component, you will see the `Profile` you created.
2.  **Add Overrides:** Click the `Add Override` button at the bottom of the `Volume` component.

#### Bloom

Bloom is essential for creating the glowing "mystic" feel for magical effects and emissive materials.

1.  **Add Bloom:** From the `Add Override` menu, select `Post-processing > Bloom`.
2.  **Enable and Configure:**
    *   Check the boxes next to `Threshold` and `Intensity` to enable them.
    *   **Threshold:** Set this to a value around `1.0`. This controls how bright a pixel needs to be to start blooming.
    *   **Intensity:** Start with a value around `0.5` and adjust to your liking. This controls the strength of the bloom effect.
    *   **Scatter:** A value of `0.7` will give a nice, soft bloom.
    *   **Tint:** You can give the bloom a slight color tint, for example, a light blue or purple, to enhance the mystic feel.

#### Tonemapping (ACES)

Tonemapping remaps the HDR colors of your scene to the LDR range of your display. ACES (Academy Color Encoding System) is a filmic tonemapper that provides a high-quality, cinematic look with vibrant colors.

1.  **Add Tonemapping:** From the `Add Override` menu, select `Post-processing > Tonemapping`.
2.  **Enable and Configure:**
    *   Check the box next to `Mode`.
    *   **Mode:** Select `ACES` from the dropdown menu. This will give your scene a more saturated and contrasted look, similar to games like Clash Royale.

#### Vignette

Vignette darkens the corners of the screen, which helps to focus the player's attention on the center of the action.

1.  **Add Vignette:** From the `Add Override` menu, select `Post-processing > Vignette`.
2.  **Enable and Configure:**
    *   Check the boxes next to `Intensity` and `Smoothness`.
    *   **Intensity:** Start with a value around `0.3`. This controls how dark the corners are.
    *   **Smoothness:** A value of `0.5` will create a soft, gradual vignette.
    *   **Color:** You can set a color for the vignette. A dark blue or purple can complement the mystic theme.

### 4. Example Configuration

Here's a summary of the recommended settings for a starting point:

*   **Bloom:**
    *   Threshold: `1.0`
    *   Intensity: `0.5`
    *   Scatter: `0.7`
    *   Tint: `(R: 200, G: 220, B: 255)`
*   **Tonemapping:**
    *   Mode: `ACES`
*   **Vignette:**
    *   Intensity: `0.3`
    *   Smoothness: `0.5`
    *   Color: `(R: 10, G: 5, B: 20)`

Remember to play with these values to find the perfect look for your game. The key is to be subtle with the effects to enhance the visuals without distracting the player.
