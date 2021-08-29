namespace KKAPI.MainGame
{
    /// <summary>
    /// Specifies the 3 buttons on the right when talking to a girl (the ones that you click to get a list of conversation topics)
    /// </summary>
    public enum TalkSceneActionKind
    {
        /// <summary>
        /// Top button, speak to the character about something.
        /// </summary>
        Talk,
        /// <summary>
        /// Middle button, listen to the character. Warning: You can't add custom buttons to this!
        /// </summary>
        Listen,
        /// <summary>
        /// Bottom button, ask the character to do something together, e.g. go exercise.
        /// </summary>
        Event
    }
}
