using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;

namespace RoA.Core;

// adapted calamity fables
public class VerletPoint(Vector2 position, bool locked = false) {
    public Vector2 Position = position, OldPosition = position;
    public bool Locked = locked;
    public int TileCollideStyle;
    public bool CollidingWithTiles => TileCollideStyle > 0;
    public bool PassThroughPlatforms => TileCollideStyle == 1;
    public Vector2? CustomGravity = null;
    public Func<Vector2, Vector2, Vector2>? CustomCollide = null;

    public static implicit operator Vector2(VerletPoint point) => point.Position;

    public static List<VerletPoint> SimpleSimulation(List<VerletPoint> segments, float segmentDistance, int loops = 10, float gravity = 0.3f, float momentumConservation = 1f) => SimpleSimulation(segments, segmentDistance, loops, gravity * Vector2.UnitY, momentumConservation);

    public static List<VerletPoint> SimpleSimulation(List<VerletPoint> segments, float segmentDistance, int loops, Vector2 gravity, float momentumConservation = 1f) {
        //https://youtu.be/PGk0rnyTa1U?t=400 verlet integration Chains reference here
        foreach (VerletPoint segment in segments) {
            if (!segment.Locked) {
                Vector2 positionBeforeUpdate = segment.Position;

                segment.Position += (segment.Position - segment.OldPosition) * momentumConservation; // This adds conservation of energy to the Segments. This makes it super bouncy and shouldnt be used but it's really funny

                if (segment.CustomGravity.HasValue)
                    segment.Position += segment.CustomGravity.Value;
                else
                    segment.Position += gravity; //=> This adds gravity to the Segments. 

                segment.OldPosition = positionBeforeUpdate;
            }
        }

        int segmentCount = segments.Count;

        for (int k = 0; k < loops; k++) {
            for (int j = 0; j < segmentCount - 1; j++) {
                VerletPoint pointA = segments[j];
                VerletPoint pointB = segments[j + 1];
                Vector2 segmentCenter = (pointA.Position + pointB.Position) / 2f;
                Vector2 segmentDirection = Utils.SafeNormalize(pointA.Position - pointB.Position, Vector2.UnitY);

                if (!pointA.Locked)
                    pointA.Position = segmentCenter + segmentDirection * segmentDistance / 2f;

                if (!pointB.Locked)
                    pointB.Position = segmentCenter - segmentDirection * segmentDistance / 2f;

                segments[j] = pointA;
                segments[j + 1] = pointB;
            }
        }

        return segments;
    }


    public static void ComplexSimulation(List<VerletPoint> points, List<VerletStick> segments, int loops = 10, float gravity = 0.3f, float momentumConservation = 1f) => ComplexSimulation(points, segments, loops, Vector2.UnitY * gravity, momentumConservation);

    public static void ComplexSimulation(List<VerletPoint> points, List<VerletStick> segments, int loops, Vector2 gravity, float momentumConservation = 1f) {
        //https://youtu.be/PGk0rnyTa1U?t=400 verlet integration Chains reference here
        foreach (VerletPoint point in points) {
            if (!point.Locked) {
                Vector2 positionBeforeUpdate = point.Position;
                Vector2 velocity = (point.Position - point.OldPosition) * momentumConservation;

                if (point.CustomGravity.HasValue)
                    velocity += point.CustomGravity.Value;
                else
                    velocity += gravity; //=> This adds gravity to the Segments. 

                if (point.CustomCollide != null)
                    velocity = point.CustomCollide(point.Position, velocity);

                if (point.CollidingWithTiles)
                    velocity = TileCollision(point.Position, velocity, point.PassThroughPlatforms);

                point.Position += velocity;
                point.OldPosition = positionBeforeUpdate;
            }
        }

        for (int k = 0; k < loops; k++) {
            foreach (VerletStick stick in segments) {
                Vector2 segmentCenter = (stick.Point1.Position + stick.Point2.Position) / 2f;
                Vector2 segmentDirection = Utils.SafeNormalize(stick.Point1.Position - stick.Point2.Position, Vector2.Zero);

                if (!stick.Point1.Locked) {
                    if (stick.Point1.CustomCollide != null)
                        stick.Point1.Position += stick.Point1.CustomCollide(stick.Point1.Position, segmentCenter + segmentDirection * stick.Length / 2f - stick.Point1.Position);
                    else if (stick.Point1.CollidingWithTiles)
                        stick.Point1.Position += TileCollision(stick.Point1.Position, segmentCenter + segmentDirection * stick.Length / 2f - stick.Point1.Position, stick.Point1.PassThroughPlatforms);
                    else
                        stick.Point1.Position = segmentCenter + segmentDirection * stick.Length / 2f;
                }

                if (!stick.Point2.Locked) {
                    if (stick.Point2.CustomCollide != null)
                        stick.Point2.Position += stick.Point2.CustomCollide(stick.Point2.Position, segmentCenter + segmentDirection * stick.Length / 2f - stick.Point2.Position);
                    else if (stick.Point2.CollidingWithTiles)
                        stick.Point2.Position += TileCollision(stick.Point2.Position, segmentCenter - segmentDirection * stick.Length / 2f - stick.Point2.Position, stick.Point2.PassThroughPlatforms);
                    else
                        stick.Point2.Position = segmentCenter - segmentDirection * stick.Length / 2f;

                }
            }
        }
    }

    public static Vector2 TileCollision(Vector2 position, Vector2 velocity, bool fallThrough) {
        Vector2 newVel = Collision.noSlopeCollision(position - new Vector2(3, 3), velocity, 6, 6, fallThrough, true);

        if (Math.Abs(newVel.X) < Math.Abs(velocity.X))
            velocity.X *= 0;

        if (Math.Abs(newVel.Y) < Math.Abs(velocity.Y))
            velocity.Y *= 0;

        return velocity;
    }

    public override string ToString() => Position.ToString() + (Locked ? " (locked)" : " (free)");
}

public class VerletStick(VerletPoint point1, VerletPoint point2) {
    public VerletPoint Point1 = point1;
    public VerletPoint Point2 = point2;
    public float Length = (point1.Position - point2.Position).Length();

    public override string ToString() => Point1.ToString() + " - " + Point2.ToString() + " (" + Length.ToString() + ")";
}

public class VerletNet {
    public List<VerletStick> Segments;
    public List<VerletPoint> Points;
    public List<VerletPoint> Extremities;
    public List<List<VerletPoint>> Chains;
    public List<float> ChainLenghts;

    public void Simulate(int iterations, float gravity) => VerletPoint.ComplexSimulation(Points, Segments, iterations, gravity);
    public void Simulate(int iterations, Vector2 gravity) => VerletPoint.ComplexSimulation(Points, Segments, iterations, gravity);
    public void Simulate(int iterations, float gravity, float momentumConservation = 1f) => VerletPoint.ComplexSimulation(Points, Segments, iterations, gravity, momentumConservation);
    public void Simulate(int iterations, Vector2 gravity, float momentumConservation = 1f) => VerletPoint.ComplexSimulation(Points, Segments, iterations, gravity, momentumConservation);

    public void Update(int iterations, float gravity, float momentumConservation = 1f) => Update(iterations, gravity * Vector2.UnitY, momentumConservation);
    public void Update(int iterations, Vector2 gravity, float momentumConservation = 1f) {
        Simulate(iterations, gravity, momentumConservation);
    }

    public void AddChain(VerletPoint point1, VerletPoint point2, int segmentCount, float droop = 0f, Func<float, Vector2>? velocity = null) {
        //Add the Extremities to the list of Points
        if (!Points.Contains(point1))
            Points.Add(point1);
        if (!Points.Contains(point2))
            Points.Add(point2);
        //Again
        if (!Extremities.Contains(point1))
            Extremities.Add(point1);
        if (!Extremities.Contains(point2))
            Extremities.Add(point2);

        List<VerletPoint> newChain = new();
        Chains.Add(newChain);
        newChain.Add(point1);

        float newChainLenght = 0;

        for (int i = 1; i < segmentCount; i++) {
            float progress = i / (float)segmentCount;
            Vector2 position = Vector2.Lerp(point1.Position, point2.Position, progress);
            if (droop != 0f)
                position.Y += MathUtils.SineBumpEasing(progress) * droop;

            if (velocity != null) {
                position += velocity(progress);
            }

            VerletPoint point = new VerletPoint(position);

            //Connect the first point to the rest of the chain
            if (i == 1)
                Segments.Add(new VerletStick(point1, point));
            //Connect the chain together
            else
                Segments.Add(new VerletStick(Points[^1], point));

            //Add the point
            Points.Add(point);
            newChain.Add(point);

            newChainLenght += Segments[^1].Length;
        }

        //Connect the last point with the end extremity to finish it off
        Segments.Add(new VerletStick(Points[^1], point2));
        newChain.Add(point2);
        newChainLenght += Segments[^1].Length;

        //Save the Length of the chain
        ChainLenghts.Add(newChainLenght);
    }

    public void AddChain(VerletPoint point1, VerletPoint point2, int segmentCount) {
        //Add the Extremities to the list of Points
        if (!Points.Contains(point1))
            Points.Add(point1);
        //Again
        if (!Extremities.Contains(point1))
            Extremities.Add(point1);
        if (!Extremities.Contains(point2))
            Extremities.Add(point2);

        List<VerletPoint> newChain = new();
        Chains.Add(newChain);
        newChain.Add(point1);

        float newChainLenght = 0;

        for (int i = 1; i < segmentCount; i++) {
            Vector2 position = Vector2.Lerp(point1.Position, point2.Position, i / (float)segmentCount);
            VerletPoint point = new VerletPoint(position);

            //Connect the first point to the rest of the chain
            if (i == 1)
                Segments.Add(new VerletStick(point1, point));
            //Connect the chain together
            else
                Segments.Add(new VerletStick(Points[^1], point));

            //Add the point
            Points.Add(point);
            newChain.Add(point);

            newChainLenght += Segments[^1].Length;
        }

        //Connect the last point with the end extremity to finish it off
        Segments.Add(new VerletStick(Points[^1], point2));
        newChain.Add(point2);
        if (!Points.Contains(point2))
            Points.Add(point2);
        newChainLenght += Segments[^1].Length;

        //Save the Length of the chain
        ChainLenghts.Add(newChainLenght);
    }


    public void AddChain(params VerletPoint[] chainPoints) {
        //Add the Extremities to the list of Points
        if (!Points.Contains(chainPoints[0]))
            Points.Add(chainPoints[0]);
        //Again
        if (!Extremities.Contains(chainPoints[0]))
            Extremities.Add(chainPoints[0]);
        if (!Extremities.Contains(chainPoints[^1]))
            Extremities.Add(chainPoints[^1]);
        List<VerletPoint> newChain = new();
        Chains.Add(newChain);
        newChain.Add(chainPoints[0]);

        float newChainLenght = 0;

        for (int i = 1; i < chainPoints.Length; i++) {
            //Connect the first point to the rest of the chain
            if (i == 1)
                Segments.Add(new VerletStick(chainPoints[0], chainPoints[i]));
            //Connect the chain together
            else
                Segments.Add(new VerletStick(Points[^1], chainPoints[i]));

            //Add the point
            Points.Add(chainPoints[i]);
            newChain.Add(chainPoints[i]);

            newChainLenght += Segments[^1].Length;
        }

        //Save the Length of the chain
        ChainLenghts.Add(newChainLenght);
    }

    public void AddSimpleStick(VerletPoint point1, VerletPoint point2) {
        //Add the Extremities to the list of Points
        if (!Points.Contains(point1))
            Points.Add(point1);
        if (!Points.Contains(point2))
            Points.Add(point2);

        List<VerletPoint> newChain = new();
        newChain.Add(point1);
        newChain.Add(point2);

        ChainLenghts.Add(point1.Position.Distance(point2.Position));
        Chains.Add(newChain);
    }

    public void RemoveFirstChain(bool ignoreShared = true) {
        List<VerletPoint> firstChain = Chains[0];

        //Remove the stored chain Length
        ChainLenghts.RemoveAt(0);

        VerletPoint start = firstChain[0];
        VerletPoint end = firstChain[firstChain.Count - 1];

        if (ignoreShared) {
            for (int i = 1; i < Chains.Count; i++)
                firstChain.RemoveAll(p => Chains[i].Contains(p));

            if (firstChain.Count == 0)
                return;

            //If the Extremities were shared, don't delete them
            if (start != firstChain[0])
                start = null;
            if (end != firstChain[firstChain.Count - 1])
                end = null;
        }

        //Remove the Points
        Points.RemoveAll(p => firstChain.Contains(p));
        //Remove the Extremities
        Extremities.RemoveAll(p => p == start || p == end);
        //Remove the sticks
        Segments.RemoveAll(s => firstChain.Contains(s.Point1) || firstChain.Contains(s.Point2));

        //Finally, clear and remove the Chains
        firstChain.Clear();
        Chains[0].Clear();
        Chains.RemoveAt(0);
    }

    public void RemoveLastChain(bool ignoreShared = true) {
        List<VerletPoint> lastChain = Chains[^1];

        //Remove the stored chain Length
        ChainLenghts.RemoveAt(ChainLenghts.Count - 1);

        VerletPoint start = lastChain[0];
        VerletPoint end = lastChain[lastChain.Count - 1];

        if (ignoreShared) {
            for (int i = 0; i < Chains.Count - 1; i++)
                lastChain.RemoveAll(p => Chains[i].Contains(p));

            if (lastChain.Count == 0)
                return;

            //If the Extremities were shared, don't delete them
            if (start != lastChain[0])
                start = null;
            if (end != lastChain[lastChain.Count - 1])
                end = null;
        }

        //Remove the Points
        Points.RemoveAll(p => lastChain.Contains(p));
        //Remove the Extremities
        Extremities.RemoveAll(p => p == start || p == end);
        //Remove the sticks
        Segments.RemoveAll(s => lastChain.Contains(s.Point1) || lastChain.Contains(s.Point2));

        //Finally, clear and remove the Chains
        lastChain.Clear();
        Chains[^1].Clear();
        Chains.RemoveAt(Chains.Count - 1);
    }

    public VerletNet() {
        Segments = new();
        Points = new();
        Extremities = new();
        Chains = new();
        ChainLenghts = new();
    }

    public void Clear() {
        Segments.Clear();
        Points.Clear();

        Extremities.Clear();

        Chains.Clear();
        ChainLenghts.Clear();
    }
}
