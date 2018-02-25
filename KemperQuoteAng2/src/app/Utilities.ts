export class Utilities {

    static vibeStateList = ['PA', 'OH'];

    static IsVibeState(state) {
        if (this.vibeStateList.includes(state))
            return true;
        else
            return false;
    }

}
