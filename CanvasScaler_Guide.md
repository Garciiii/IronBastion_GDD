# Canvas Scaler Guide for a "Clash Royale" Style UI

To achieve a responsive UI that maintains a consistent layout across various aspect ratios, similar to games like Clash Royale, it's crucial to configure the `Canvas Scaler` component correctly. Here are the recommended settings and best practices.

### Recommended `Canvas Scaler` Settings

On your main `Canvas` GameObject, find the `Canvas Scaler` component and apply the following settings:

*   **UI Scale Mode:** `Scale With Screen Size`
    *   This is the most important setting. It ensures that your UI elements scale up or down based on the screen's resolution.

*   **Reference Resolution:** `1080 x 1920` (or a similar 9:16 portrait aspect ratio)
    *   This should be the resolution you are primarily developing and testing in. A common mobile portrait resolution is a good choice.

*   **Screen Match Mode:** `Match Width Or Height`
    *   This mode allows you to control how the UI scales when the screen's aspect ratio differs from the reference aspect ratio.

*   **Match:** `0.5` (Balanced)
    *   A `Match` value of `0` will make the UI scale only with the width of the screen.
    *   A `Match` value of `1` will make the UI scale only with the height of the screen.
    *   A value of `0.5` provides a balanced scaling between width and height, which is generally the best choice for supporting a wide range of devices. It prevents the UI from becoming too large on wide screens or too small on tall screens.

### UI Hierarchy and Layout Groups

To ensure your UI adapts correctly, you should use a combination of `Layout Groups` and `Anchors`.

*   **Horizontal Layout Group for the Card Deck:**
    *   For the bottom deck of tower cards, use a `Horizontal Layout Group` on the parent panel.
    *   This will automatically arrange the cards horizontally.
    *   Enable `Control Child Size` for both width and height to ensure the cards are uniformly sized.
    *   Use `Spacing` to add padding between the cards.

*   **Anchoring:**
    *   **Bottom Deck:** Anchor the deck panel to the bottom of the screen. Use the `Bottom` anchor preset.
    *   **Top Bar (Gold, Elixir, Wave):** Anchor the top bar to the top of the screen. Use the `Top` anchor preset.
    *   **Side Elements:** Anchor elements to the sides of the screen if they should stick to the edges.

### Example Hierarchy

```
Canvas
├── SafeAreaPanel (with SafeAreaHandler.cs)
│   ├── TopBarPanel (Anchored to Top)
│   │   ├── Gold_UI
│   │   ├── Elixir_UI
│   │   └── Wave_UI
│   ├── BottomDeckPanel (Anchored to Bottom)
│   │   └── HorizontalLayoutGroup
│   │       ├── TowerCard_1
│   │       ├── TowerCard_2
│   │       └── ...
│   └── Other_UI_Elements
└── WorldSpaceCanvas (for health bars, etc.)
```

By following these guidelines, you can create a robust and responsive UI that looks great on a wide variety of devices, maintaining the polished feel of a "Premium Stylized 3D" game.
