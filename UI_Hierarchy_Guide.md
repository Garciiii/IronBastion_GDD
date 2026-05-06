# UI Hierarchy and 9-Slicing Guide

This guide provides a step-by-step walkthrough for setting up the UI hierarchy for your Tower Defense game, focusing on the card system. It also explains how to configure 9-slicing for your card backgrounds to create scalable, non-distorted borders.

## Part 1: UI Hierarchy Setup

This setup uses a combination of Panels and Layout Groups to create a responsive and organized UI.

1.  **Create the Main Canvas:**
    *   In the Hierarchy, right-click -> `UI` -> `Canvas`. Name it `MainCanvas`.
    *   Select `MainCanvas` and in the Inspector, find the `Canvas Scaler` component.
    *   Set `UI Scale Mode` to `Scale With Screen Size`.
    *   Set `Reference Resolution` to `1080 x 1920`.
    *   Set `Screen Match Mode` to `Match Width Or Height` with a `Match` of `0.5`.

2.  **Create the Deck Panel:**
    *   Right-click on `MainCanvas` -> `UI` -> `Panel`. Name it `DeckPanel`.
    *   This panel will hold the tower cards at the bottom of the screen.
    *   **Anchoring:** With `DeckPanel` selected, go to the `Rect Transform` component in the Inspector. Click the anchor preset box and, while holding `Alt` and `Shift`, click the `bottom-stretch` preset. This will anchor the panel to the bottom of the screen and stretch it horizontally.
    *   **Sizing:** Adjust the `Height` of the `DeckPanel` to your desired size (e.g., `250`).
    *   **Background:** You can set the `Source Image` of the `Image` component to a background sprite for your deck or set the color to transparent.

3.  **Add a Horizontal Layout Group:**
    *   Select `DeckPanel` and click `Add Component` in the Inspector. Search for and add a `Horizontal Layout Group`.
    *   This component will automatically arrange the tower cards inside the panel.
    *   **Configuration:**
        *   `Padding`: Add some padding to the `Left`, `Right`, `Top`, and `Bottom` to give the cards some space from the edges of the panel.
        *   `Spacing`: Set a value for `Spacing` (e.g., `20`) to create a gap between the cards.
        *   `Child Alignment`: Set to `Middle Center` to center the cards within the panel.
        *   `Control Child Size`: Check `Width` and `Height` to make all cards the same size.

4.  **Create the Tower Card Prefab:**
    *   Inside the `DeckPanel`, create a UI Image (`Right-click` -> `UI` -> `Image`). Name it `TowerCard`. This will be your card prefab.
    *   **Add Components:**
        *   Add the `TowerCard.cs` script to this GameObject.
        *   Add a `Button` component if you want click feedback, but the `TowerCard.cs` script already handles pointer events.
    *   **Structure the Card:**
        *   Inside `TowerCard`, create another `Image` for the `TowerIcon`.
        *   Create two `TextMeshPro - Text` objects for the `Cost` and `Level`.
        *   Position and style these elements to create your desired card layout.
    *   **Assign References:** Drag the `TowerIcon`, `Cost`, and `Level` UI elements to the corresponding fields in the `TowerCard.cs` script in the Inspector.
    *   **Create Prefab:** Drag the `TowerCard` GameObject from the Hierarchy into your Project window to create a prefab. You can now delete the `TowerCard` from the Hierarchy and instantiate it from your code or place multiple instances in the `DeckPanel`.

## Part 2: 9-Slicing for Card Borders

9-slicing is a technique that allows you to scale a UI image without distorting its borders. This is perfect for card backgrounds with wooden or stone frames.

1.  **Import Settings for the Card Background:**
    *   Select your card background sprite in the Project window.
    *   In the Inspector, change the `Texture Type` to `Sprite (2D and UI)`.
    *   Click the `Sprite Editor` button.

2.  **Configure 9-Slicing in the Sprite Editor:**
    *   In the Sprite Editor window, you will see your image.
    *   You will see four green lines that define the slicing. Drag these lines to separate the corners, the top/bottom borders, and the left/right borders from the center of the image.
    *   The corners will not be scaled.
    *   The top and bottom borders will be stretched horizontally.
    *   The left and right borders will be stretched vertically.
    *   The center will be stretched both horizontally and vertically.
    *   Adjust the `Border` values in the Inspector (within the Sprite Editor) to be precise.
    *   Click `Apply` in the Sprite Editor to save the changes.

3.  **Apply the 9-Sliced Sprite to the UI Image:**
    *   Select your `TowerCard` prefab and find the `Image` component for the card background.
    *   Set the `Source Image` to your 9-sliced sprite.
    *   Change the `Image Type` from `Simple` to `Sliced`.

Now, when you resize the `TowerCard`, the borders will remain crisp and un-distorted, while the center of the card scales to fill the space. This is essential for creating a polished and professional-looking UI that adapts to different screen sizes.
