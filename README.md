# Build your own 2D Platformer!

This

## Building the Platforms

In the Hierarchy window, right click and add a new 2D Object -> Tilemap.  Rename this Tilemap "Platforms."  Open the tile palette window from the menu (Window -> 2D -> Tile Palette).  This window should be docked on the right.

In the Tile Palette window, select the tile palette you wish to use:
   - Castle
   - Desert
   - Grass
   - Graveyard
   - Nature
   - Night
   - SciFi
   - Snow

Use the "Paint with active brush" ('b') tool to start drawing tiles.  Select the tile (or groups of tiles) in the palette.  Open the Scene view (Window -> General -> Scene), and start drawing your level.  You can delete any tiles using the "Erase with active brush" tool ('d'), if necessary.  This part is up to you!  Design a level as complicated or simple as you like.

Optionally, you can add additional Tilemap objects for foreground and background objects.  Be sure to switch active tilemaps in the Tile Palette window.  If you do, be sure to set the Tilemap Renderer's "Order in Layer" for each Tilemap, accordingly.  Higher numbers will position the layer on top of lower numbers.

Select the Platforms object in the Hierarchy window, and look for the Layer field in the Inspector window.  Clicking this list should give you an option to "Add Layer"  Next to "User Layer 9" add the text "Ground"  Select the Platforms Tilemap again, and choose "Ground" from the list for Layer, to select our newly created layer.  This is the layer that our player will use to determine which objects it should land on when falling.

- Select the "Platfoms" Tilemap object in the Hierarchy window, and add a Tilemap Collider 2D component in the inspector.  This will add a many colliders to the platforms, so that our character won't just fall through them.
- This is rather inefficient, so we'll also add a new Composite Collider 2D.  In the Tilemap Collider 2D, select "Used by Composite" so that the Composite Collider 2D combines these colliders.
- The Composite Collider 2D also adds a Rigidbody 2D component, so that our platforms interact with the physics system.  In the Rigidbody 2D component, set the Body Type to "Static", since our platforms will not be affected by gravity.

Finally, once our player is jumping round, we want to be able to jump up through the platforms, but still land on top.  Unity has a component for this, too:  Platform Effector 2D.  To let the collider be used with the Platform Effector 2D, in the Composite Collider 2D, enable "Used by Effector"

## Adding our Character

Now, we need to add our character.  First, you must choose the character that you want to use from the Characters folder.  Once you have decided, drag any of the sprite images into the Hierarchy Window.  Rename this object "Player", and position it above one of your platforms.  In the Sprite Renderer component, set the "Order in Layer" to 3.

If we run the game now, our character will just hover in mid-air.  That is because it isn't physics enabled.  Let's change that now.  First, add a Capsule Collider 2D, so the object will hit our platforms when falling.  Next, add a Rigidbody 2D component, and make sure the Body Type is set to "Dynamic"  In Constraints, enable "Freeze Rotation" in the Z axis, so our character doesn't fall over from the friction of moving along the ground.  In the Capsule Collider 2D, click "Edit Collider" and change the bounds of the capsule shape to roughly outline the player's body and head (excluding their limbs).

Now, when we run our game, the player will fall, and then land onto the platform!

## Controls and Physics

If we want our character to move in response to our inputs, we'll need to add a controller and write some code.

- Add a Character Controller 2D component to the Player object
- For "Ground Layers" select the "Ground" layer that we created earlier
- Note the location of the Player object in the Inspector window
- Add a new empty game object in the Hierarchy window, named "Ceiling", at the same position as the player
   - Adjust the position on the Y-axis so that it is at the top of the head of your character
   - Drag this object into the Ceiling Position of the Character Controller 2D component
   - Add the Ceiling object as a child of our Player
- Add a new empty game object in the Hierarchy window, named "Ground", at the same position as the player
   - Adjust the position on the Y-axis so that it is at the bottom of the feet of your character
   - Drag this object into the Ground Position of the Character Controller 2D component
   - Add the Ground object as a child of our Player

Now, we'll add our own code to move the player around.  In the Project window, navigate into the Scripts folder, where you will see the Character Controller 2D script we've just used.  Create a new script by right-clicking in this window (Create -> C# script), and call it "PlayerInput"  Erase the default code, and replace it with the following:

~~~~
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public float runSpeed = 40f;

    private CharacterController2D controller;
    private Animator animator;

    private float horizontalMove = 0f;
    private bool jump = false;

    private void Start() {
        controller = GetComponent<CharacterController2D>();
        animator = GetComponent<Animator>();
    }

    void Update() {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetButtonDown("Jump")) {
            jump = true;
            animator.SetTrigger("Jump");
        }

        animator.SetFloat("Speed", controller.speed);
    }

    void FixedUpdate() {
        // move our character
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
        jump = false;
    }
}
~~~~

We'll add this component to our Player object, by adding a new component of type "Player Input"

Test out the level!  Use the cursor to move, and space to jump.

Note:  You may need to adjust the jumping force and the run speed variables in the Inspector for our Player in order to tweak the gameplay just right.

Since our camera doesn't move, it could make our game difficult to play.  To make the camera move with our player, add it as a child of our Player.

## Animations

We have a working game, but it is pretty boring without any animations!

### Idle Animation

- Select the Player object in the Hierarchy window, and add a new "Animator" component
- With the Player selected in the Hierarchy window, open the Animation window (Window -> Animation -> Animation)
   - This window should be docked with the Scene view, for now
- Click the "Create" button to create a new animation clip and an animator controller
- You will be prompted to select a filename, create a new Folder called "Animations" and call the animation "Idle"
- Navigate to your character's folder, and select all of the Idle animation sprites(shift click to multi-select)
   - Drag these sprites into the Animation window (on the right side; the timeline).  This will create a new animation frame for each sprite.
   - Slow down the animation by setting the Samples to 10
- Open the Animator window (Window -> Animation -> Animator) to see the animation state machine
   - You'll see that there is a state "Idle" and a transition to it.  Both are orange, meaning that the Idle animation will be the character's default state.

### Getting Ready

Run the game to try out our animated character.  Back in the Animation window, create some additional animation clips:
   - Jump:  Using the frames of the Jump animation
   - Walk:  Using the frames of the Walk animation (some characters don't have walk, but run will work fine instead)

In order to invoke these animations, our code will need to tell Unity's Animation system (MecAnim) that we want to change states.  For this, we need parameters.  In the Parameters tab, create two new parameters:
   - Jump (trigger)
   - Speed (float)

### Jump Animation

In the Animator window, create a new transition by right-clicking the "Any State" state and select "Make Transition", which will connect to the Jump state.  Click the arrow that is created to edit its properties.  We want this transition to happen when the Jump parameter is set.  Under conditions, click + to add a new one.  Verify that "Has Exit Time" is not enabled.  Finally, select "Jump"

To transition back to the Idle state, create another transition from Jump to Idle, without any conditions.

We also do not want our animation to loop.  Double click the Jump state in the Animator window and disable "Loop Time"

### Walk Animation

Our walk animation will be enabled when the speed of our character is greater than 0.01 (to allow for any un-precise controls, e.g. analog sticks).  Add a transition between Idle and Walk, and set its "Has Exit Time" to false, and its condition to Speed Greater than 0.01.  Similarly, add a transition from Walk to Idle, with its "Has Exit Time" to false, and its condition to Speed Less than 0.01.

## Wrap-up

That is it.  What a good start!

To make this game even better, you could add an enemy character (e.g. a Zombie) with a Capsule Collider 2D set as a Trigger, which will cause some script to execute when touched.  This could injure your player, cause knockback, deduct points, or even kill the enemy.  You could also add a menu, and a heads up display (score, health, etc.).
