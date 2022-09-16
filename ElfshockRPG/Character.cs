using System;
using System.Collections.Generic;
using System.Text;

namespace ElfshockRPG
{
    public class Character
    {
        public string Name { get; set; }
        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int Agility { get; set; }
        public int Range { get; set; }
        public char Sprite { get; set; }
        public int Health, Mana, Damage, posX = 0, posY = 0;

        public Character(string name, int strength, int agility, int intelligence, int range, char sprite, int newX = 0, int newY = 0)
        {
            Name = name;
            Strength = strength;
            Agility = agility;
            Intelligence = intelligence;
            Range = range;
            Sprite = sprite;
            posX = newX;
            posY = newY;

            Setup();
        }

        public Character() { }

        public void Setup()
        {
            this.Health = this.Strength * 5;
            this.Mana = this.Intelligence * 3;
            this.Damage = this.Agility * 2;
        }

        public void SetXY(int newX, int newY)
        {
            posX = newX;
            posY = newY;
        }

        public void AI(Character target, int[,] grid)
        {
            int dx = target.posX - this.posX;
            int dy = target.posY - this.posY;

            if (Math.Abs(dx) + Math.Abs(dy) == 1)
            {
                target.Health -= this.Damage;
                return;
            }

            int newX = this.posX, newY = this.posY;

            if (dx == 0) newY += Math.Sign(dy);
            else if (dy == 0) newX += Math.Sign(dx);
            else
            {
                newX += Math.Sign(dx);
            }

            if (grid[newX, newY] == 0)
            this.SetXY(newX, newY);
        }
    }
}
